import org.apache.directory.ldap.client.api.LdapNetworkConnection
//TODO cleanup imports
import org.apache.directory.api.ldap.model.exception.LdapException
import org.apache.directory.api.ldap.model.name.Dn
import org.apache.directory.api.ldap.model.cursor.{
  CursorException,
  EntryCursor
}
import org.apache.directory.api.ldap.model.entry.Entry
import org.apache.directory.api.ldap.model.message.SearchScope
import io.circe.{Encoder, Json}
import io.circe.parser._
import io.circe.syntax.EncoderOps
import org.apache.directory.api.ldap.model.entry.Attribute
import org.apache.directory.api.ldap.model.entry.Value

import scala.util.Try

object ActiveDirectoryClient {
  case class ADUser(
      samaccountname: String,
      emplid: String,
      givenname: String,
      sn: String,
      mail: String,
      physicaldeliveryofficename: String,
      phone: String,
      cell: String
  )

  implicit val encodeADUser: Encoder[ADUser] = (user: ADUser) => {
    Json.obj(
      "samaccountname" -> Json.fromString(user.samaccountname),
      "emplid"         -> Json.fromString(user.emplid),
      "givenname"      -> Json.fromString(user.givenname),
      "sn"             -> Json.fromString(user.sn),
      "mail"           -> Json.fromString(user.mail),
      "physicaldeliveryofficename" -> Json.fromString(
        user.physicaldeliveryofficename
      ),
      "phone" -> Json.fromString(user.phone),
      "cell"  -> Json.fromString(user.cell)
    )
  }
  implicit val encodeVectorADUser: Encoder[Vector[ADUser]] =
    (vec: Vector[ADUser]) => {
      vec.map { _.asJson(encodeADUser) }.asJson
    }

  def getVal(key: String, e: Entry): String = {
    try {
      e.get(key).get().getString
    } catch {
      case exception: NullPointerException => ""
    }
  }

  class ActiveDirectoryClient(
      private var HOST: String,
      private var PORT: Int,
      private var SSL: Boolean
  ) {
    private final var connection =
      new LdapNetworkConnection(this.HOST, this.PORT, this.SSL)

    private final var CACHE_LIMIT: Int  = 1500;
    final var CALL_CACHE: Vector[Entry] = Vector[Entry]()
    final var DEFAULT_SEARCH_BASE: String = "<OU TO START IN HERE>"

    def apply(
        host: String,
        port: Int,
        ssl: Boolean
    ): ActiveDirectoryClient = {
      this.HOST = host
      this.PORT = port
      this.SSL = ssl
      this.connection =
        new LdapNetworkConnection(this.HOST, this.PORT, this.SSL)
      this
    }

    def close(): Unit = {
      connection.close()
    }

    def login(
        dn: String,
        password: String
    ): Either[LdapException, ActiveDirectoryClient] = {
      try {
        connection.connect()
        connection.bind(dn, password)
        Right(this)
      } catch {
        case e: LdapException => Left(e)
      }
    }

    def search(
        query: String,
        scope: String,
        base: String
    ): Either[Unit, (Vector[Entry], ActiveDirectoryClient)] = {
      //this method needs to be chainable
      //we return a tuple of the results from the most recent ldap call
      //and the current client object

      //look at threading oe futures here

      val typeMap = Map(
        "users"  -> "(objectclass=user)",
        "groups" -> "(objectclass=group)",
        "ou"     -> "(objectclass=organizationalUnit)"
      )
      var _entries = Vector[Entry]()
      try {
        val ldapQuery = s"(&${typeMap(scope)}(${query
          .substring(1, query.lastIndexOf(')'))}))"

        val cursor: EntryCursor =
          connection.search(
            {
              if (base.equals("")) new Dn(DEFAULT_SEARCH_BASE)
              else new Dn(base)
            },
            ldapQuery,
            SearchScope.SUBTREE
          )

        while (cursor.next()) {
          try {
            _entries ++= Vector(cursor.get)
          } catch {
            case t: CursorException =>
              t.printStackTrace()
          }
        }
      } catch {
        case e: LdapException =>
          new Error("Ldap Query Error:\n" + e.getMessage)
            .printStackTrace()
          Left(close())
      }

      if (CALL_CACHE.length < CACHE_LIMIT)
        CALL_CACHE ++= _entries
      Right((_entries, this))
    }
  }
}
