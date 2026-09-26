import { property as g, customElement as v } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase as U, UmbUfmElementBase as b, UMB_UFM_RENDER_CONTEXT as E } from "@umbraco-cms/backoffice/ufm";
import { UmbId as x } from "@umbraco-cms/backoffice/id";
import { UMB_MEDIA_ENTITY_TYPE as P } from "@umbraco-cms/backoffice/media";
import { U as w } from "./item-name-resolver-Bscftqrs.js";
import { a as k } from "./link-value.function-D3PQjIif.js";
var C = Object.defineProperty, A = Object.getOwnPropertyDescriptor, u = (t) => {
  throw TypeError(t);
}, l = (t, e, r, s) => {
  for (var n = s > 1 ? void 0 : s ? A(e, r) : e, i = t.length - 1, a; i >= 0; i--)
    (a = t[i]) && (n = (s ? a(e, r, n) : a(n)) || n);
  return s && n && C(e, r, n), n;
}, d = (t, e, r) => e.has(t) || u("Cannot " + r), I = (t, e, r) => (d(t, e, "read from private field"), r ? r.call(t) : e.get(t)), p = (t, e, r) => e.has(t) ? u("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, r), y = (t, e, r) => (d(t, e, "access private method"), r), f, o, _, h;
const M = /^(\w+)(?:\s*:\s*(\d+))?$/, T = /,\s*(["'])(.*)\1\s*$/, N = /<\/?[a-z][^>]*>/i;
let c = class extends b {
  constructor() {
    super(), p(this, o), p(this, f, new w(this)), this.consumeContext(E, (t) => {
      this.observe(
        t?.value,
        async (e) => {
          this.value = await y(this, o, _).call(this, e);
        },
        "observeValue"
      );
    });
  }
};
f = /* @__PURE__ */ new WeakMap();
o = /* @__PURE__ */ new WeakSet();
_ = async function(t) {
  const { entries: e, fallback: r } = O(this.aliases);
  if (!t || typeof t != "object") return r;
  for (const s of e) {
    const n = await y(this, o, h).call(this, t[s.alias]);
    if (n) return F(n, s.length);
  }
  return r;
};
h = async function(t) {
  const e = q(t);
  if (e.length)
    return I(this, f).names(
      e[0].type,
      e.map((r) => r.unique)
    );
  if (t && typeof t == "object" && "markup" in t)
    return m(String(t.markup ?? ""));
  if (typeof t == "string")
    return N.test(t) ? m(t) : t.trim();
  if (Array.isArray(t))
    return t.filter((r) => typeof r == "string" && r.trim() !== "").join(", ");
  if (typeof t == "number" && t !== 0)
    return String(t);
};
l([
  g()
], c.prototype, "aliases", 2);
c = l([
  v("ufm-first-value")
], c);
function O(t) {
  const e = t ?? "", r = T.exec(e);
  return { entries: (r ? e.slice(0, r.index) : e).split(",").map((i) => M.exec(i.trim())).filter((i) => i !== null).map(([, i, a]) => ({ alias: i, length: a ? Number(a) : void 0 })), fallback: r?.[2] ?? "" };
}
function q(t) {
  const r = (typeof t == "string" ? t.split(",") : Array.isArray(t) ? t : []).map(D);
  return r.length && r.every((s) => s) ? r : [];
}
function D(t) {
  if (typeof t == "string") {
    const n = t.trim();
    return x.validate(n) ? { unique: n } : k(n);
  }
  if (!t || typeof t != "object") return;
  const { mediaKey: e, unique: r, type: s } = t;
  if (e) return { type: P, unique: e };
  if (r) return { type: s, unique: r };
}
function m(t) {
  return (new DOMParser().parseFromString(t, "text/html").body.textContent ?? "").trim();
}
function F(t, e) {
  return !e || t.length <= e ? t : `${t.slice(0, e).trim()}…`;
}
class Y extends U {
  render(e) {
    return e.text ? `<ufm-first-value aliases="${S(e.text)}"></ufm-first-value>` : void 0;
  }
}
function S(t) {
  return t.replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;");
}
export {
  Y as UfmFirstValueComponent,
  c as UfmFirstValueElement,
  Y as api
};
//# sourceMappingURL=first-value.component-39kfqO-c.js.map
