var unamit = {

  sid: undefined,

  names: [],
  gender: ['', 'male', 'female', 'unisex'],
  ratings: { no: -10, doubtful: 0, probably: 7, yes: 10 },

  elements: {
    msg: undefined,
    loader: undefined,
    container: undefined,
    names: {}
  },

  load: {
    count: 0,
    start: () => {
      unamit.load.count++;
      unamit.elements.loader.slideDown();
    },
    end: (f) => {
      unamit.load.count -= f ? unamit.load.count : 1;
      if (unamit.load.count === 0) unamit.elements.loader.slideUp('slow');
    }
  },

  json: (url, options) => {
    unamit.load.start();
    $.ajax(url, $.extend({}, {
      type: 'get',
      dataType: 'json',
      contentType: 'application/json; charset=UTF-8',
      headers: { 'Authorization': unamit.sid },
      data: JSON.stringify(options.value),
      error: (r) => {
        unamit.elements.msg.empty();
        if (r.status === 401) {
          r.statusText = 'Invalid credentials';
          if (unamit.sid !== undefined) unamit.logout(true);
        }
        else if (r.status === 429) r.statusText = 'Too many requests - please wait';
        unamit.elements.msg.append($(`<span class="error">${r.statusText}.</span>`));
      },
      complete: unamit.load.end
    }, options));
  },

  init: () => {
    unamit.sid = unamit.sid || Cookies.get('session');
    unamit.elements.msg = unamit.elements.msg || $('#msg');
    unamit.elements.loader = unamit.elements.loader || $('#loader');
    unamit.elements.container = unamit.elements.container || $('#container');

    unamit.elements.msg.empty();
    unamit.elements.container.empty();

    if (unamit.sid === undefined) return unamit.showLogin();

    unamit.getUser();
    unamit.getNames(5);
  },

  showLogin: () => {
    unamit.load.end(true);
    unamit.elements.container.append($('<form onsubmit="unamit.login(); return false;"><input id="id" type="email" placeholder="Email Address" required /><input id="password" type="password" placeholder="Password" required /><input type="submit" value="Log in" /></form>'));
  },

  login: () => {
    var success = (r) => { Cookies.set('session', r.id, { expires: 1 / 3 }); unamit.init(); };
    unamit.json('/sessions', { type: 'post', value: { id: $('#id').val(), password: $('#password').val() }, success: success });
  },

  logout: (i) => {
    if (i !== true) unamit.json('/sessions', { type: 'delete', data: null, headers: { 'Authorization': t }, error: null, complete: unamit.logout });
    else {
      Cookies.remove('session');
      unamit.sid = undefined;
      unamit.init();
    }
  },

  getUser: () => {
    var success = (r) => {
      $(`<span class="info">${r.id} <i class="logout" onclick="unamit.logout(); return false;">x</i><span onclick="unamit.elements.partnerPanel.toggle('slow');">${(r.partner === null ? 'link partner' : r.partner + (r.mutual === 0 ? ' <i class="notmutual">?</i>' : ''))}</span></span>`).appendTo(unamit.elements.msg);
      unamit.elements.partnerPanel = $(`<span id="partnerPanel" class="info"><span><input type="submit" value="Ok" onclick="unamit.addPartner(); return false;" /></span><span><input id="partner" type="email" placeholder="Partner email" value="${(r.partner === null ? '' : r.partner)}" required /></span><br style="clear: both" /></span>`).hide().appendTo(unamit.elements.msg);
    };
    unamit.json('/users/me', { success: success });
  },

  getNames: (c) => {
    var success = (r) => { for (var i = 0; i < r.length; i++) unamit.addName(r[i]); };
    unamit.json('/names', { data: { limit: c, exclude: unamit.names }, success: success });
  },

  addName: (e) => {
    var name, wrapper = $(`<div></div>`)
      .hide().appendTo(unamit.elements.container)
      .append($(`<a class="no" href="#" onclick="unamit.rate('${e.id}', unamit.ratings.no); return false;"></a><a class="doubtful" href="#" onclick="unamit.rate('${e.id}', unamit.ratings.doubtful); return false;"></a>`))
      .append(name = $(`<div class="name ${unamit.gender[e.gender]}">${e.id}</div>`))
      .append($(`<a class="probably" href="#" onclick="unamit.rate('${e.id}', unamit.ratings.probably); return false;"></a><a class="yes" href="#" onclick="unamit.rate('${e.id}', unamit.ratings.yes); return false;"></a>`));

    unamit.names.push(e.id);
    unamit.elements.names[e.id] = wrapper;

    swipe.initElements(name);
    wrapper.show();
  },

  rate: (name, value) => {
    var success = () => {
      unamit.names.splice(unamit.names.indexOf(name), 1);
      unamit.elements.names[name].remove();
      unamit.elements.names[name] = undefined;
      unamit.getNames(1);
    };
    unamit.json('/users/me/ratings', { type: 'post', value: { name: name, value: value }, success: success });
  },

  addPartner: () => {
    unamit.json('/users/me', { type: 'put', value: { partner: $('#partner').val() }, success: unamit.init });
  }
};

$('document').ready(unamit.init);