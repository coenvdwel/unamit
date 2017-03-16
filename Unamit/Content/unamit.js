var unamit = {

  sid: undefined,

  names: {},
  gender: ['', 'male', 'female', 'unisex'],
  ratings: { no: -10, doubtful: 0, probably: 7, yes: 10 },

  elements: {
    msg: undefined,
    loader: undefined,
    container: undefined
  },

  loader: {
    count: 0,
    start: () => {
      unamit.loader.count++;
      unamit.elements.loader.slideDown();
    },
    end: (force) => {
      unamit.loader.count -= force === true ? unamit.loader.count : 1;
      if (unamit.loader.count === 0) unamit.elements.loader.slideUp('slow');
    }
  },

  json: (url, options) => {
    unamit.loader.start();
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
        unamit.elements.msg.append($(`<div class="error">${r.statusText}.</div>`));
      },
      complete: unamit.loader.end
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

    unamit.user();
    unamit.load(5);
  },

  showLogin: () => {
    unamit.loader.end(true);
    unamit.elements.container.append($('<form class="login" onsubmit="unamit.login(); return false;"><input id="id" type="email" placeholder="Email Address" required /><input id="password" type="password" placeholder="Password" required /><input type="submit" value="Log in" /><div class="register">New user? Register <a href="#" onclick="unamit.showRegister(); return false;">here</a>.</div></form>'));
  },

  showRegister: () => {
    unamit.elements.container.empty();
    unamit.elements.container.append($('<form class="login" onsubmit="unamit.register(); return false;"><input id="id" type="email" placeholder="Email Address" required /><input id="password" type="password" placeholder="Password" required /><input type="submit" value="Register" /></form>'));
  },

  register: () => {
    unamit.json('/users', { type: 'post', value: { id: $('#id').val(), password: $('#password').val() }, success: unamit.login });
  },

  login: () => {
    var success = (r) => { Cookies.set('session', r.id, { expires: 1 / 3 }); unamit.init(); };
    unamit.json('/sessions', { type: 'post', value: { id: $('#id').val(), password: $('#password').val() }, success: success });
  },

  logout: (local) => {
    if (local === undefined) unamit.json('/sessions', { type: 'delete', data: null, error: null, complete: unamit.logout });
    else {
      Cookies.remove('session');
      unamit.sid = undefined;
      unamit.init();
    }
  },

  user: () => {
    var success = (r) => {
      $(`<div class="info"><a href="#" onclick="unamit.elements.menu.toggle('slow'); return false;">${r.id}</a></div>`).appendTo(unamit.elements.msg);

      unamit.elements.menu = $(`<div class="info"></div>`).hide().appendTo(unamit.elements.msg);
      unamit.elements.menu.append($(`<form onsubmit="unamit.partner(); return false;"><label for="partner">Partner</label><input type="submit" value="Ok" /><div><input id="partner" type="email" placeholder="Partner email" value="${(r.partner === null ? '' : r.partner)}" ${(r.partner !== null && r.mutual === 0 ? 'style="color: #F78181;" ' : '')}/></div></form>`));
      unamit.elements.menu.append($(`<form onsubmit="unamit.name(); return false;"><label for="name">Add name</label><input type="submit" value="Ok" /><div><input id="name" type="text" placeholder="Name" required /></div></form>`));
      unamit.elements.menu.append($(`<form onsubmit="unamit.password(); return false;"><label for="old">Password</label><input type="submit" value="Ok" /><div><input id="old" type="password" placeholder="Old" required /><input id="new" type="password" placeholder="New" required /></div></form>`));
      unamit.elements.menu.append($(`<form onsubmit="unamit.logout(); return false;" class="logout"><div><input type="submit" value="Log out" /></div></form>`));
    };
    unamit.json('/users/me', { success: success });
  },

  ids: () => {
    var ids = [];
    $.each(unamit.names, function (id, value) { ids.push(id); });
    return ids;
  },

  load: (show) => {
    var success = (r) => {
      for (var i = 0; i < r.length; i++) unamit.names[r[i].id] = r[i];
      if (show !== undefined) unamit.show(show);
    };
    unamit.json('/names', { data: { exclude: unamit.ids() }, success: success });
  },

  show: (count) => {
    count = count || 1;
    $.each(unamit.names, function (id, value) {
      if (count <= 0) return false;
      if (value.shown === true) return true;

      count--;
      unamit.render(value);
    });

    if (unamit.ids().length < 10) unamit.load();
  },

  render: (value) => {
    value.shown = true;
    value.wrapper = $(`<div></div>`)
      .hide().appendTo(unamit.elements.container)
      .append($(`<a class="no" href="#" onclick="unamit.rate('${value.id}', unamit.ratings.no); return false;"></a><a class="doubtful" href="#" onclick="unamit.rate('${value.id}', unamit.ratings.doubtful); return false;"></a>`))
      .append(value.element = $(`<div class="name ${unamit.gender[value.gender]}">${value.id}</div>`))
      .append($(`<a class="probably" href="#" onclick="unamit.rate('${value.id}', unamit.ratings.probably); return false;"></a><a class="yes" href="#" onclick="unamit.rate('${value.id}', unamit.ratings.yes); return false;"></a>`));

    swipe.initElements(value.element);
    value.wrapper.show();
  },

  rate: (name, value) => {
    var success = () => {
      unamit.names[name].wrapper.slideUp();
      delete unamit.names[name];
      unamit.show();
    };
    unamit.json('/users/me/ratings', { type: 'post', value: { name: name, value: value }, success: success });
  },

  partner: () => {
    unamit.json('/users/me', { type: 'put', value: { partner: $('#partner').val() }, success: unamit.init });
  },

  name: () => {
    unamit.json('/names/' + $('#name').val(), {
      success: (r) => {
        unamit.names[r.id] = r;
        unamit.render(r);
        unamit.elements.menu.toggle('slow');
      }
    });
  },

  password: () => {
    unamit.json('/users/me', { type: 'put', value: { password: $('#new').val() }, success: unamit.init });
  }
};

$('document').ready(unamit.init);