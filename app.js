//snippets from an rxjs project

var r = rxjs;
var o = r.operators;
var fromEvent = r.fromEvent;
var of = r.of;
var from = r.from;

var select = function (a) {
  return document.getElementById(a);
};

//some wrapper methods
var html = function (a, b, append) {
  if (append === false || append === undefined) {
    select(a).innerHTML = b;
  } else {
    select(a).innerHTML += b;
  }
};

var markup = function (arr) {
  return arr.join("");
};

var key = function (elem, eventCallback) {
  if (elem === "document") {
    fromEvent(document, "keydown").subscribe(function (eventTarget) {
      eventCallback(eventTarget);
    });
  } else {
    fromEvent(select(elem), "keydown").subscribe(function (eventTarget) {
      eventCallback(eventTarget);
    });
  }
};

var click = function (elem, eventCallback) {
  if (elem === "document") {
    fromEvent(document, "click").subscribe(function (eventTarget) {
      eventCallback(eventTarget); //elem.addEventListener("click",e=>{})
    });
  } else {
    fromEvent(select(elem), "click").subscribe(function (eventTarget) {
      eventCallback(eventTarget);
    });
  }
};

//uneeded, but maybe useful
var ObservableFactory = {
  newObserverable: function (cb) {
    return new r.Observable(cb);
  },
};

var netObservable = function (httpMethod, url) {
  return ObservableFactory.newObserverable(function (subscriber) {
    var _subscriber = subscriber;
    axios({
      method: httpMethod,
      url: url,
    }).then(function (response) {
      if (response.data.length != undefined) {
        subscriber.next(from(response.data));
      } else {
        _subscriber.next(of(response.data));
      }
    });
  });
};


var users = netObservable("get","/users")

//flatten nested observables and observe the inner observable.
var observeNestedObservable = function (observable, cb, completionCb) {
  observable.subscribe({
    next: function (_observable) {
      _observable.subscribe({
        next: function (nestedObject) {
          if (nestedObject.length !== undefined) {
            observeNestedObservable(
              ObservableFactory.newObserverable(function (subscriber) {
                subscriber.next(from(nestedObject));
              }),
              cb(nestedObject),
              completionCb()
            );
          } else {
            cb(nestedObject);
          }
        },
        error: function (err) {
          console.error(err);
        },
        complete: function () {
          completionCb();
        },
      });
    },
    error: function (e) {
      console.error(e);
    },
    complete: function () { },
  });
};

//cache collection from backend. 
var _users = []

observeNestedObservable(
  users,
  function (user) {
    _users.push(user);
    html("tableInject", HTML_TEMPLATES.userRow(user), true); //helper function to produce markup from an object
  },
  function () { }
);


var specialKeys = [
  "CapsLock",
  "Home",
  "Tab",
  "Control",
  "Enter",
  "Shift",
  "ArrowLeft",
  "ArrowRight",
  "ArrowUp",
  "ArrowDown",
  "(",
  ")",
  "{",
  "}",
  "[",
  "]",
  "`",
  "<",
  ">",
  "/",
  "\\",
  '"',
  ";",
]


// search routine, still refining this bit.
key("search", function (event) {
  var bad = false;
  var inputString = event.target.value;

  var state = document.getElementById("search").value

  if (event.target.value.length < 2) {
    return;
  }
  if (inputString.length < 2) {
    return;
  }
  if (specialKeys.includes(event.key)) {
    bad = true;
  }
  if (event.key === "Backspace") {
    if (event.target.value.length <= 1 || inputString === "") {
      inputString = "";
      return;
    } else {
      inputString = event.target.value.substring(
        0,
        event.target.value.length - 1
      );
    }
  } else {
    inputString += event.key;
  }
  if (!bad && inputString.length > 2) {
    html("tableInject", "", false);
    from(
      _users.filter(function (x) {
        switch (inputString.includes(" ")) {
          case true: {
            return (
              (x.fn
                .toLowerCase()
                .includes(inputString.split(" ")[0].toLowerCase()) &&
                x.ln
                  .toLowerCase()
                  .includes(inputString.split(" ")[1].toLowerCase())) ||
              x.title.toLowerCase().includes(inputString.toLowerCase())
            );
            break;
          }
          case false: {
            return (
              ((x.fn[0].toString().toLowerCase() === inputString[0].toLowerCase()) &&
                x.fn.toLowerCase().includes(inputString.toLowerCase())) ||
              ((x.ln[0].toString().toLowerCase() === inputString[0].toLowerCase()) &&
                x.ln.toLowerCase().includes(inputString.toLowerCase())) ||
              (x.fn.toLowerCase().includes(inputString.toLowerCase()) ||
                x.ln.toLowerCase().includes(inputString.toLowerCase())) ||
              x.title.toLowerCase().includes(inputString.toLowerCase()) ||

              (x.fn.toLowerCase()[0].concat(x.ln.toLowerCase()).includes(inputString.toLowerCase()))

            );
            break;
          }
        }
      })
    ).subscribe(function (x) {
      html("tableInject", HTML_TEMPLATES.userRow(x), true);
    });
  } else {
    return;
  }
});

