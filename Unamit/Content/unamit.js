var msg;
var loader;
var body;
var sid;

var gender = ['', 'male', 'female', 'unisex'];

var json = (url, options) => {
  loader.slideDown();
  $.ajax(url, $.extend({}, {
    type: 'get',
    dataType: 'json',
    contentType: 'application/json; charset=UTF-8',
    headers: { 'Authorization': sid },
    data: JSON.stringify(options.value),
    error: (r) => {
      msg.empty(); loader.slideUp();
      if (r.status == 401) {
        r.statusText = 'Invalid credentials';
        if (sid != undefined) logout(true);
      }
      else if (r.status == 429) r.statusText = 'Too many requests - please wait';
      msg.append($(`<span class="error">${r.statusText}.</span>`));
    }
  }, options));
};

var login = () => {
  var value = { id: $('#id').val(), password: $('#password').val() };
  var success = (r) => { Cookies.set('session', r.id, { expires: 1 / 3 }); load(); };
  json('/sessions', { type: 'post', value: value, success: success });
  return false;
};

var logout = (i) => {
  var t = sid;
  sid = undefined; Cookies.remove('session');
  if (i === true) load(); else json('/sessions', { type: 'delete', data: null, headers: { 'Authorization': t }, error: null, complete: load });
  return false;
};

var addPartner = () => {
  json('/users/me', { type: 'put', value: { partner: $('#partner').val() }, success: load });
  return false;
}

var rate = (name, value, e) => {
  var success = (r) => {
    loader.slideUp('slow');
    $(e).parent().remove();
    // todo: get new, append
  };
  json('/users/me/ratings', { type: 'post', value: { name: name, value: value }, success: success });
  return false;
};

var load = () => {

  msg = msg || $('#msg');
  loader = loader || $('#loader');
  body = body || $('#body');
  sid = sid || Cookies.get('session');

  body.empty();
  msg.empty();

  if (sid == undefined) {
    loader.slideUp();
    return body.append($('<form onsubmit="return login();"><input id="id" type="email" placeholder="Email Address" required /><input id="password" type="password" placeholder="Password" required /><input type="submit" value="Log in" /></form>'));
  }

  json('/users/me', {
    success: (r) => {
      msg.append($(`<span class="info">${r.id} <i class="logout" onclick="logout();">x</i><span onclick="$('#partnerPanel').toggle('slow');">${(r.partner == null ? 'link partner' : r.partner + (r.mutual == 0 ? ' <i class="notmutual">?</i>' : ''))}</span></span>`));
      $(`<span id="partnerPanel" class="info"><span><input type="submit" value="Ok" onclick="addPartner();" /></span><span><input id="partner" type="email" placeholder="Partner email" value="${(r.partner == null ? '' : r.partner)}" required /></span><br style="clear: both" /></span>`).hide().appendTo(msg);
    }
  });

  json('/names', {
    success: (r) => {
      loader.slideUp('slow');
      for (var i = 0; i < r.length; i++) {
        var name, wrapper = $(`<div></div>`)
          .hide().appendTo(body)
          .append($(`<a class="no" href="#" onclick="rate('${r[i].id}', -10, this);"></a><a class="doubtful" href="#" onclick="rate('${r[i].id}', 0, this);"></a>`))
          .append(name = $(`<div class="name ${gender[r[i].gender]}">${r[i].id}</div>`))
          .append($(`<a class="probably" href="#" onclick="rate('${r[i].id}', 7, this);"></a><a class="yes" href="#" onclick="rate('${r[i].id}', 10, this);"></a>`));

        swipe.initElements(name);
        wrapper.show();
      }
    }
  });

};

$('document').ready(load);