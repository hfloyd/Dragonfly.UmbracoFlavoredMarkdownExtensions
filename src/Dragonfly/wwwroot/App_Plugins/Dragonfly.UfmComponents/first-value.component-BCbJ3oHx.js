import { property as h, customElement as d } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase as U, UmbUfmElementBase as w, UMB_UFM_RENDER_CONTEXT as x } from "@umbraco-cms/backoffice/ufm";
import { U as y, a as E } from "./describe-value-D_vm_1vY.js";
import { p as C } from "./link-value.function-D3PQjIif.js";
var N = Object.defineProperty, b = Object.getOwnPropertyDescriptor, u = (e) => {
  throw TypeError(e);
}, p = (e, t, r, n) => {
  for (var s = n > 1 ? void 0 : n ? b(t, r) : t, a = e.length - 1, i; a >= 0; a--)
    (i = e[a]) && (s = (n ? i(t, r, s) : i(s)) || s);
  return n && s && N(t, r, s), s;
}, v = (e, t, r) => t.has(e) || u("Cannot " + r), V = (e, t, r) => (v(e, t, "read from private field"), r ? r.call(e) : t.get(e)), m = (e, t, r) => t.has(e) ? u("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, r), k = (e, t, r) => (v(e, t, "access private method"), r), c, o, _;
const P = /^(\w+)(\.name)?(?:\s*:\s*(\d+))?$/, O = /,\s*(["'])(.*)\1\s*$/;
let l = class extends w {
  constructor() {
    super(), m(this, o), m(this, c, new y(this)), this.consumeContext(x, (e) => {
      this.observe(
        e?.value,
        async (t) => {
          this.value = await k(this, o, _).call(this, t);
        },
        "observeValue"
      );
    });
  }
};
c = /* @__PURE__ */ new WeakMap();
o = /* @__PURE__ */ new WeakSet();
_ = async function(e) {
  const { entries: t, fallback: r } = g(this.aliases);
  if (!e || typeof e != "object") return r;
  for (const n of t) {
    const s = e[n.alias], a = n.linkName ? F(s) : await E(s, V(this, c));
    if (a) return M(a, n.length);
  }
  return r;
};
p([
  h()
], l.prototype, "aliases", 2);
l = p([
  d("ufm-first-value")
], l);
function g(e) {
  const t = e ?? "", r = O.exec(t);
  return { entries: (r ? t.slice(0, r.index) : t).split(",").map((a) => P.exec(a.trim())).filter((a) => a !== null).map(([, a, i, f]) => ({ alias: a, linkName: !!i, length: f ? Number(f) : void 0 })), fallback: r?.[2] ?? "" };
}
function F(e) {
  return C(e)[0]?.name?.trim() || void 0;
}
function M(e, t) {
  return !t || e.length <= t ? e : `${e.slice(0, t).trim()}…`;
}
class T extends U {
  render(t) {
    return t.text ? `<ufm-first-value aliases="${$(t.text)}"></ufm-first-value>` : void 0;
  }
}
function $(e) {
  return e.replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;");
}
export {
  T as UfmFirstValueComponent,
  l as UfmFirstValueElement,
  T as api
};
//# sourceMappingURL=first-value.component-BCbJ3oHx.js.map
