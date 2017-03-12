var body;
var msg;
var sid;

var json = (url, options) => {
  $.ajax(url, $.extend({}, {
    type: 'get',
    dataType: 'json',
    contentType: 'application/json; charset=UTF-8',
    headers: { 'Authorization': sid },
    data: JSON.stringify(options.value),
    error: (r) => {
      msg.empty();
      if (r.status == 401) {
        r.statusText = "Invalid credentials";
        if (sid != undefined) { Cookies.remove('session'); load(); }
      }
      else if (r.status == 429) r.statusText = "Too many requests - please wait";
      msg.append($(`<span class="error">${r.statusText}.</span>`));
    }
  }, options));
};

var login = () => {
  var value = { id: $("#id").val(), password: $("#password").val() };
  var success = (r) => { Cookies.set('session', r.id, { expires: 1 / 3 }); load(); };
  json('/sessions', { type: 'post', value: value, success: success });
  return false;
};

var load = () => {

  body = $('#body');
  msg = $('#msg');
  sid = Cookies.get('session');

  body.empty();
  msg.empty();

  if (sid == undefined) {
    var form = $('<form onsubmit="return login();"></form>').appendTo(body);
    form.append($('<input id="id" type="email" placeholder="Email Address" required>'));
    form.append($('<input id="password" type="password" placeholder="Password" required>'));
    return form.append($('<input type="submit" value="Log in">'));
  }

  json('/users/me', {
    success: (r) => {
      var info = $(`<span class="info">${r.id}</span>`).appendTo(msg);
      if (r.partner != null) info.append($(`<span>${r.partner}${r.mutual == 0 ? ' (?)' : ''}</span>`));
    }
  });

  json('/names', {
    success: (r) => {
      var gender = ['', 'male', 'female', 'unisex'];
      for (var i = 0; i < r.length; i++) {
        body.append($(`<div class="name ${gender[r[i].gender]}">${r[i].id}</div>`));
      }
    }
  });

};

$('document').ready(load);
