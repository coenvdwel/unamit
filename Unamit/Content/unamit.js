var unamit = {

  /* Session variables */

  sid: undefined,
  resultView: false,
  gender: 0,

  /* Constants */

  genders: ['', 'male', 'female', 'unisex'],
  ratings: {
    no: -10, doubtful: 0, probably: 7, yes: 10,
    '-10': 'no', '0': 'doubtful', '7': 'probably', '10': 'yes'
  },

  /* Cache */

  names: {},

  elements: {
    menu: undefined,
    menuElement: undefined,
    message: undefined,
    loader: undefined,
    container: undefined
  },

  /* Helper methods */

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
        if (r.status === 401) return (unamit.sid !== undefined) ? unamit.logout(true) : unamit.elements.message.append($(`<div class="error">Invalid credentials.</div>`));
        else if (r.status === 429) return unamit.elements.message.append($(`<div class="error">Too many requests - please wait...</div>`));
        else {
          unamit.elements.container.empty();
          unamit.elements.message.append($(`<div class="error">Error. Please reload and try again.</div>`));
        }
      },
      complete: unamit.loader.end
    }, options));
  },

  ids: (fn) => {
    var ids = [];
    $.each(unamit.names, (id, value) => { if (fn === undefined || fn(value)) ids.push(id); });
    return ids;
  },

  /* Initialisation */

  init: () => {
    unamit.sid = unamit.sid || Cookies.get('session');
    unamit.resultView = false;

    unamit.elements.menu = (unamit.elements.menu || $('#menu')).empty();
    unamit.elements.message = (unamit.elements.message || $('#message')).empty();
    unamit.elements.loader = unamit.elements.loader || $('#loader');
    unamit.elements.container = (unamit.elements.container || $('#container')).empty();

    if (unamit.sid === undefined) return unamit.showLogin();

    unamit.menu();
    unamit.load();
  },

  /* Login / logout functionality */

  showLogin: () => {
    unamit.loader.end(true);
    unamit.elements.container.append($('<form class="login" onsubmit="unamit.login(); return false;"><input id="id" type="email" placeholder="Email Address" required /><input id="password" type="password" placeholder="Password" required /><input type="submit" value="Log in" /><div class="register">New user? Register <a href="#" onclick="unamit.showRegister(); return false;">here</a>.</div></form>'));
  },

  login: () => {
    unamit.json('/sessions', { type: 'post', value: { id: $('#id').val(), password: $('#password').val() }, success: (r) => { Cookies.set('session', r.id, { expires: 1 / 3 }); unamit.init(); } });
  },

  logout: (local) => {
    if (local === undefined) unamit.json('/sessions', { type: 'delete', data: null, error: null, complete: unamit.logout });
    else {
      Cookies.remove('session');
      unamit.sid = undefined;
      unamit.init();
    }
  },

  /* Register functionality */

  showRegister: () => {
    unamit.elements.container.empty();
    unamit.elements.container.append($('<form class="login" onsubmit="unamit.register(); return false;"><input id="id" type="email" placeholder="Email Address" required /><input id="password" type="password" placeholder="Password" required /><input type="submit" value="Register" /></form>'));
  },

  register: () => {
    unamit.json('/users', { type: 'post', value: { id: $('#id').val(), password: $('#password').val() }, success: unamit.login });
  },

  /* Menu functionality */

  menu: () => {
    unamit.json('/users/me', {
      success: (r) => {
        $(`<div><a href="#" onclick="unamit.elements.menuElement.toggle('slow'); return false;">${r.id}</a><a href="#" onclick="unamit.results(this); return false;">Results</a></div>`).appendTo(unamit.elements.menu);
        unamit.elements.menuElement = $(`<div></div>`).hide().appendTo(unamit.elements.menu);
        unamit.elements.menuElement.append($(`<form class="stretch"><div><input type="radio" onclick="unamit.setGender(this);" name="gender" value="0" class="unisex" ${unamit.gender === 0 ? 'checked ' : ''}/><input type="radio" onclick="unamit.setGender(this);" name="gender" value="1" class="male" ${unamit.gender === 1 ? 'checked ' : ''}/><input type="radio" onclick="unamit.setGender(this);" name="gender" value="2" class="female" ${unamit.gender === 2 ? 'checked ' : ''}/></div></form>`));
        unamit.elements.menuElement.append($(`<form onsubmit="unamit.partner(); return false;"><label for="partner">Partner</label><input type="submit" value="Ok" /><div><input id="partner" type="email" placeholder="Partner email" value="${(r.partner === null ? '' : r.partner)}" ${(r.partner !== null && r.mutual === 0 ? 'style="color: #F78181;" ' : '')}/></div></form>`));
        unamit.elements.menuElement.append($(`<form onsubmit="unamit.name(); return false;"><label for="name">Add name</label><input type="submit" value="Ok" /><div><input id="name" type="text" placeholder="Name" required /></div></form>`));
        unamit.elements.menuElement.append($(`<form onsubmit="unamit.password(); return false;"><label for="old">Password</label><input type="submit" value="Ok" /><div><input id="old" type="password" placeholder="Old" required /><input id="new" type="password" placeholder="New" required /></div></form>`));
        unamit.elements.menuElement.append($(`<form onsubmit="unamit.logout(); return false;" class="stretch"><div><input type="submit" value="Log out" /></div></form>`));
      }
    });
  },

  /* Menu handlers */

  setGender: (e) => {
    unamit.gender = $(e).val();
    unamit.names = {};
    unamit.elements.container.empty();
    unamit.resultView = false;
    unamit.load();
  },

  partner: () => {
    unamit.json('/users/me', { type: 'put', value: { partner: $('#partner').val() }, success: unamit.init });
  },

  name: () => {
    unamit.json('/names/' + $('#name').val(), {
      success: (r) => {
        unamit.names[r.id] = r;
        unamit.render(r);
        unamit.elements.menuElement.hide('slow');
      }
    });
  },

  password: () => {
    unamit.json('/users/me', { type: 'put', value: { password: $('#new').val() }, success: unamit.init });
  },

  results: (e) => {
    unamit.elements.menuElement.hide('slow');
    unamit.elements.container.empty();
    unamit.resultView = !unamit.resultView;
    $(e).css({ color: unamit.resultView ? "#F78181" : "white" });

    if (!unamit.resultView) return unamit.show();
    $.each(unamit.names, function (_, value) { if (value.shown === true) value.shown = false; });

    unamit.json('/users/me/ratings', {
      success: (r) => {
        for (var i = 0; i < r.length; i++) $('<div></div>').appendTo(unamit.elements.container)
          .append(r[i].partnerValue === null ? undefined : $(`<div class="value ${unamit.ratings[r[i].partnerValue]}">P</div>`))
          .append(r[i].value === null ? undefined : $(`<div class="value ${unamit.ratings[r[i].value]}">U</div>`))
          .append($(`<div class="name ${unamit.genders[r[i].gender]}">${r[i].id}</div>`));
      }
    });
  },

  /* Load, show and render names */

  load: () => {
    unamit.json('/names', {
      data: { gender: unamit.gender, exclude: unamit.ids() }, success: (r) => {
        for (var i = 0; i < r.length; i++) unamit.names[r[i].id] = r[i];
        if (!unamit.resultView) unamit.show();
      }
    });
  },

  show: () => {
    var count = 5 - unamit.ids((value) => { return value.shown === true; }).length;
    $.each(unamit.names, function (_, value) {
      if (count <= 0) return false;
      if (value.shown === true) return true;

      count--;
      unamit.render(value);
    });

    if (unamit.ids().length < 7) unamit.load();
  },

  render: (value) => {
    value.shown = true;
    value.wrapper = $(`<div></div>`)
      .hide().appendTo(unamit.elements.container)
      .append($(`<a class="no" href="#" onclick="unamit.rate('${value.id}', unamit.ratings.no); return false;"></a><a class="doubtful" href="#" onclick="unamit.rate('${value.id}', unamit.ratings.doubtful); return false;"></a>`))
      .append(value.element = $(`<div class="name ${unamit.genders[value.gender]}">${value.id}</div>`))
      .append($(`<a class="probably" href="#" onclick="unamit.rate('${value.id}', unamit.ratings.probably); return false;"></a><a class="yes" href="#" onclick="unamit.rate('${value.id}', unamit.ratings.yes); return false;"></a>`));

    swipe.initElements(value.element);
    value.wrapper.show();
  },

  /* Name (rating) handler */

  rate: (name, value) => {
    unamit.json('/users/me/ratings', {
      type: 'post', value: { name: name, value: value }, success: () => {
        unamit.names[name].wrapper.slideUp();
        delete unamit.names[name];
        unamit.show();
      }
    });
  }
};

$('document').ready(unamit.init);