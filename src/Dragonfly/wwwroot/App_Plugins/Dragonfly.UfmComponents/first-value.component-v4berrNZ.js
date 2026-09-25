import { property as v, customElement as g } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase as U, UmbUfmElementBase as b, UMB_UFM_RENDER_CONTEXT as E } from "@umbraco-cms/backoffice/ufm";
import { UmbId as x } from "@umbraco-cms/backoffice/id";
import { UMB_MEDIA_ENTITY_TYPE as P } from "@umbraco-cms/backoffice/media";
import { U as w } from "./item-name-resolver-Bscftqrs.js";
import { a as k } from "./link-value.function-D3PQjIif.js";
var C = Object.defineProperty, I = Object.getOwnPropertyDescriptor, u = (t) => {
  throw TypeError(t);
}, l = (t, e, r, s) => {
  for (var n = s > 1 ? void 0 : s ? I(e, r) : e, i = t.length - 1, a; i >= 0; i--)
    (a = t[i]) && (n = (s ? a(e, r, n) : a(n)) || n);
  return s && n && C(e, r, n), n;
}, d = (t, e, r) => e.has(t) || u("Cannot " + r), M = (t, e, r) => (d(t, e, "read from private field"), r ? r.call(t) : e.get(t)), m = (t, e, r) => e.has(t) ? u("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, r), _ = (t, e, r) => (d(t, e, "access private method"), r), f, o, y, h;
const T = /^(\w+)(?:\s*:\s*(\d+))?$/, A = /,\s*(["'])(.*)\1\s*$/, N = /<\/?[a-z][^>]*>/i;
let c = class extends b {
  constructor() {
    super(), m(this, o), m(this, f, new w(this)), this.consumeContext(E, (t) => {
      this.observe(
        t?.value,
        async (e) => {
          this.value = await _(this, o, y).call(this, e);
        },
        "observeValue"
      );
    });
  }
};
f = /* @__PURE__ */ new WeakMap();
o = /* @__PURE__ */ new WeakSet();
y = async function(t) {
  const { entries: e, fallback: r } = O(this.aliases);
  if (!t || typeof t != "object") return r;
  for (const s of e) {
    const n = await _(this, o, h).call(this, t[s.alias]);
    if (n) return F(n, s.length);
  }
  return r;
};
h = async function(t) {
  const e = q(t);
  if (e.length)
    return M(this, f).names(
      e[0].type,
      e.map((r) => r.unique)
    );
  if (t && typeof t == "object" && "markup" in t)
    return p(String(t.markup ?? ""));
  if (typeof t == "string")
    return N.test(t) ? p(t) : t.trim();
  if (typeof t == "number" && t !== 0)
    return String(t);
};
l([
  v()
], c.prototype, "aliases", 2);
c = l([
  g("ufm-first-value")
], c);
function O(t) {
  const e = t ?? "", r = A.exec(e);
  return { entries: (r ? e.slice(0, r.index) : e).split(",").map((i) => T.exec(i.trim())).filter((i) => i !== null).map(([, i, a]) => ({ alias: i, length: a ? Number(a) : void 0 })), fallback: r?.[2] ?? "" };
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
function p(t) {
  return (new DOMParser().parseFromString(t, "text/html").body.textContent ?? "").trim();
}
function F(t, e) {
  return !e || t.length <= e ? t : `${t.slice(0, e).trim()}…`;
}
class j extends U {
  render(e) {
    return e.text ? `<ufm-first-value aliases="${S(e.text)}"></ufm-first-value>` : void 0;
  }
}
function S(t) {
  return t.replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;");
}
export {
  j as UfmFirstValueComponent,
  c as UfmFirstValueElement,
  j as api
};
//# sourceMappingURL=first-value.component-v4berrNZ.js.map
