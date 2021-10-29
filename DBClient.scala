import java.nio.charset.Charset

import org.bson.BsonValue
import org.mongodb.scala.bson.BsonValue
import org.mongodb.scala.connection.ClusterSettings
import org.mongodb.scala.{Document, MongoClient, MongoCollection, MongoCredential, MongoDatabase, Observer, ServerAddress, Subscription}
import com.mongodb.MongoClientSettings
import org.mongodb.scala.result.InsertOneResult

object DB {

  case class PostResult(
                         var id:String,
                         var postSuccess:Boolean
                       )

  def login(username: String, password: String): Boolean = {
    val uri: String = ""
    val client = MongoClient(uri)
    val db = client.getDatabase("application_users")
    val collection = db.getCollection("users")
    var validUser = false;
    Thread.sleep(1000);

    collection.find(Document("userId" -> username.toLowerCase())).first().subscribe(
      new Observer[Document] {
        override def onNext(result: Document): Unit = {
          if (password.equals(result.get("passKey").get.asString().getValue)) {
            validUser = true
          } else {
            validUser = false
          }
        }
        override def onError(e: Throwable): Unit = println("error")

        override def onComplete(): Unit = {}
    })

    Thread.sleep(1000);
    client.close()
    validUser
  }

  def postExpense(expense: Expense) : PostResult = {
    val uri: String = ""
    val client = MongoClient(uri)
    val db = client.getDatabase("billing")
    val collection = db.getCollection("bills")
    Thread.sleep(1000)

    var pResult = PostResult("",false)

    collection.insertOne(
      Document(
        "name" -> expense.name,
        "amount" -> expense.amount.asInstanceOf[String],
        "expenseType" -> expense.expenseType,
        "dateSubmitted" -> expense.postDate
      )
    ).subscribe(new Observer[InsertOneResult] {
      override def onNext(result: InsertOneResult): Unit = pResult.id = result.getInsertedId.asObjectId().getValue.toString

      override def onError(e: Throwable): Unit = println("failure")

      override def onComplete(): Unit = {
        pResult.postSuccess = true
        println(
          "[PostExpense] - Complete"
        )
      }
    })

    Thread.sleep(1000)
    client.close()
    pResult
  }

  def collectAllExpenses() : Vector[Expense] = {
      val uri: String = ""
    val client = MongoClient(uri)
    val db = client.getDatabase("billing")
    val collection = db.getCollection("bills")
    Thread.sleep(1000)

    var expenses =  Vector[Expense]()

    collection.find().subscribe(new Observer[Document] {
      override def onNext(result: Document): Unit = {
        val amount = result.get("amount") match {
          case Some(value) => {
              try {
                value.asDouble().getValue()
              }
              catch {
                case e: Exception => {
                  e.printStackTrace()
                  value.asString().getValue()
                }
              }
          }
          case None => {
            None
          }

        }
        expenses ++= Vector(
          Expense(
            result.get("name").get.asString().getValue(),
            amount,
            result.get("expenseType").get.asString().getValue(),
            result.get("dateSubmitted").get.asString().getValue()
          )
        )
      }

      override def onError(e: Throwable): Unit = {
          e.printStackTrace()
      }

      override def onComplete(): Unit = println(
        "[AllExpenses] - Complete"
      )
    })

    Thread.sleep(1000)
    client.close()
    expenses
  }
}
