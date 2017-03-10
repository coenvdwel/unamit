var send = (url, type, data, success, error) => {
  $.ajax({
    url: url,
    type: type,
    dataType: 'json',
    contentType: 'application/json; charset=UTF-8',
    data: JSON.stringify(data),
    error: error,
    success: success
  });
};

var login = () => {

  var success = (r) => {
    Cookies.set('session', r.id);
    load();
  };

  var error = () => {
    alert('Invalid.');
  };

  send('/sessions', 'post', { id: $("#id").val(), password: $("#password").val() }, success, error);
  return false;
};

var load = () => {

  var body = $('#body');
  var sid = Cookies.get('session');

  body.empty();

  if (sid == undefined) {

    var form = $('<form onsubmit="return login();"></form>').appendTo(body);

    form.append($('<div><label for="id">Id</label><input id="id" name="id" type="text" required></div>'));
    form.append($('<div><label for="password">Password</label><input id="password" name="password" type="password" required></div>'));
    form.append($('<div><input type="submit" value="Log in"></div>'));

    return;
  }

  $.ajaxSetup({ headers: { 'Authorization': sid } });

  body.append($('<p>Normal site</p>'));
  send('/users/me/partner', 'get', null, (r) => body.append('<p>'+r+'</p>'));

};

$('document').ready(load);