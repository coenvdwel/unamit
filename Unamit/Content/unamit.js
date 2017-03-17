var unamit = {

  sid: undefined,

  names: {},
  resultView: false,
  gender: ['', 'male', 'female', 'unisex'],
  ratings: { no: -10, doubtful: 0, probably: 7, yes: 10 },
  ratingValues: { '-10': 'no', '0': 'doubtful', '7': 'probably', '10': 'yes' },

  elements: {
    menu: undefined,
    message: undefined,
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
        unamit.elements.message.empty();
        if (r.status === 401) {
          if (unamit.sid !== undefined) return unamit.logout(true);
          r.statusText = 'Invalid credentials';
        }
        else if (r.status === 429) r.statusText = 'Too many requests - please wait';
        else unamit.elements.container.empty();
        unamit.elements.message.append($(`<div class="error">${r.statusText}.</div>`));
      },
      complete: unamit.loader.end
    }, options));
  },

  init: () => {
    unamit.sid = unamit.sid || Cookies.get('session');
    unamit.elements.menu = unamit.elements.menu || $('#menu');
    unamit.elements.message = unamit.elements.message || $('#message');
    unamit.elements.loader = unamit.elements.loader || $('#loader');
    unamit.elements.container = unamit.elements.container || $('#container');

    unamit.elements.menu.empty();
    unamit.elements.message.empty();
    unamit.elements.container.empty();

    unamit.resultView = false;

    if (unamit.sid === undefined) return unamit.showLogin();

    unamit.menu();
    unamit.load();
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

  menu: () => {
    var success = (r) => {
      $(`<div><a href="#" onclick="unamit.elements.menu.toggle('slow'); return false;">${r.id}</a><a href="#" onclick="unamit.toggleResults(this); return false;">Results</a></div>`).appendTo(unamit.elements.menu);

      unamit.elements.menu = $(`<div></div>`).hide().appendTo(unamit.elements.menu);
      unamit.elements.menu.append($(`<form onsubmit="unamit.partner(); return false;"><label for="partner">Partner</label><input type="submit" value="Ok" /><div><input id="partner" type="email" placeholder="Partner email" value="${(r.partner === null ? '' : r.partner)}" ${(r.partner !== null && r.mutual === 0 ? 'style="color: #F78181;" ' : '')}/></div></form>`));
      unamit.elements.menu.append($(`<form onsubmit="unamit.name(); return false;"><label for="name">Add name</label><input type="submit" value="Ok" /><div><input id="name" type="text" placeholder="Name" required /></div></form>`));
      unamit.elements.menu.append($(`<form onsubmit="unamit.password(); return false;"><label for="old">Password</label><input type="submit" value="Ok" /><div><input id="old" type="password" placeholder="Old" required /><input id="new" type="password" placeholder="New" required /></div></form>`));
      unamit.elements.menu.append($(`<form onsubmit="unamit.logout(); return false;" class="logout"><div><input type="submit" value="Log out" /></div></form>`));
    };
    unamit.json('/users/me', { success: success });
  },

  toggleResults: (e) => {
    unamit.elements.menu.hide('slow');
    unamit.elements.container.empty();
    unamit.resultView = !unamit.resultView;
    $(e).css({ color: unamit.resultView ? "#F78181" : "white" });

    if (!unamit.resultView) return unamit.show();
    $.each(unamit.names, function (id, value) { if (value.shown === true) value.shown = false; });
    
    var success = (r) => {
      for (var i = 0; i < r.length; i++) unamit.elements.container.append($(`<div><div class="value ${unamit.ratingValues[r[i].partnerValue]}">P</div><div class="value ${unamit.ratingValues[r[i].value]}">U</div><div class="name ${unamit.gender[r[i].gender]}">${r[i].id}</div></div>`));
    };
    unamit.json('/users/me/ratings', { success: success });
  },

  ids: (fn) => {
    var ids = [], fn = fn || (() => { return true; });
    $.each(unamit.names, (id, value) => { if (fn(value)) ids.push(id); });
    return ids;
  },

  load: () => {
    var success = (r) => {
      for (var i = 0; i < r.length; i++) unamit.names[r[i].id] = r[i];
      if(!unamit.resultView) unamit.show();
    };
    unamit.json('/names', { data: { exclude: unamit.ids() }, success: success });
  },

  show: () => {
    var count = 5 - unamit.ids((v) => { return v.shown === true; }).length;
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