import { property as v, customElement as U } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase as g, UmbUfmElementBase as E, UMB_UFM_RENDER_CONTEXT as w } from "@umbraco-cms/backoffice/ufm";
import { UmbId as P } from "@umbraco-cms/backoffice/id";
import { UMB_MEDIA_ENTITY_TYPE as C } from "@umbraco-cms/backoffice/media";
import { U as b } from "./item-name-resolver-Bscftqrs.js";
import { a as x } from "./link-value.function-D3PQjIif.js";
var I = Object.defineProperty, M = Object.getOwnPropertyDescriptor, u = (t) => {
  throw TypeError(t);
}, l = (t, e, r, i) => {
  for (var n = i > 1 ? void 0 : i ? M(e, r) : e, a = t.length - 1, o; a >= 0; a--)
    (o = t[a]) && (n = (i ? o(e, r, n) : o(n)) || n);
  return i && n && I(e, r, n), n;
}, d = (t, e, r) => e.has(t) || u("Cannot " + r), T = (t, e, r) => (d(t, e, "read from private field"), r ? r.call(t) : e.get(t)), f = (t, e, r) => e.has(t) ? u("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, r), _ = (t, e, r) => (d(t, e, "access private method"), r), c, s, y, h;
const N = /^(\w+)(?:\s*:\s*(\d+))?$/, O = /<\/?[a-z][^>]*>/i;
let p = class extends E {
  constructor() {
    super(), f(this, s), f(this, c, new b(this)), this.consumeContext(w, (t) => {
      this.observe(
        t?.value,
        async (e) => {
          this.value = await _(this, s, y).call(this, e);
        },
        "observeValue"
      );
    });
  }
};
c = /* @__PURE__ */ new WeakMap();
s = /* @__PURE__ */ new WeakSet();
y = async function(t) {
  if (!t || typeof t != "object") return "";
  for (const e of k(this.aliases)) {
    const r = await _(this, s, h).call(this, t[e.alias]);
    if (r) return D(r, e.length);
  }
  return "";
};
h = async function(t) {
  const e = q(t);
  if (e.length)
    return T(this, c).names(
      e[0].type,
      e.map((r) => r.unique)
    );
  if (t && typeof t == "object" && "markup" in t)
    return m(String(t.markup ?? ""));
  if (typeof t == "string")
    return O.test(t) ? m(t) : t.trim();
  if (typeof t == "number" && t !== 0)
    return String(t);
};
l([
  v()
], p.prototype, "aliases", 2);
p = l([
  U("ufm-first-value")
], p);
function k(t) {
  return (t ?? "").split(",").map((e) => N.exec(e.trim())).filter((e) => e !== null).map(([, e, r]) => ({ alias: e, length: r ? Number(r) : void 0 }));
}
function q(t) {
  const r = (typeof t == "string" ? t.split(",") : Array.isArray(t) ? t : []).map(A);
  return r.length && r.every((i) => i) ? r : [];
}
function A(t) {
  if (typeof t == "string") {
    const n = t.trim();
    return P.validate(n) ? { unique: n } : x(n);
  }
  if (!t || typeof t != "object") return;
  const { mediaKey: e, unique: r, type: i } = t;
  if (e) return { type: C, unique: e };
  if (r) return { type: i, unique: r };
}
function m(t) {
  return (new DOMParser().parseFromString(t, "text/html").body.textContent ?? "").trim();
}
function D(t, e) {
  return !e || t.length <= e ? t : `${t.slice(0, e).trim()}…`;
}
class Y extends g {
  render(e) {
    return e.text ? `<ufm-first-value aliases="${F(e.text)}"></ufm-first-value>` : void 0;
  }
}
function F(t) {
  return t.replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;");
}
export {
  Y as UfmFirstValueComponent,
  p as UfmFirstValueElement,
  Y as api
};
//# sourceMappingURL=first-value.component-B6x9VB6k.js.map
