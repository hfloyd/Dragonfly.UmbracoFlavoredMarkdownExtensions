import { property as d, customElement as y } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase as U, UmbUfmElementBase as w, UMB_UFM_RENDER_CONTEXT as E } from "@umbraco-cms/backoffice/ufm";
import { U as C } from "./item-name-resolver-Bscftqrs.js";
import { p as x, w as D } from "./link-value.function-D3PQjIif.js";
var N = Object.defineProperty, O = Object.getOwnPropertyDescriptor, c = (e) => {
  throw TypeError(e);
}, f = (e, t, a, r) => {
  for (var s = r > 1 ? void 0 : r ? O(t, a) : t, i = e.length - 1, o; i >= 0; i--)
    (o = e[i]) && (s = (r ? o(t, a, s) : o(s)) || s);
  return r && s && N(t, a, s), s;
}, u = (e, t, a) => t.has(e) || c("Cannot " + a), P = (e, t, a) => (u(e, t, "read from private field"), a ? a.call(e) : t.get(e)), m = (e, t, a) => t.has(e) ? c("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, a), h = (e, t, a) => (u(e, t, "access private method"), a), l, n, v, _;
let p = class extends w {
  constructor() {
    super(), m(this, n), m(this, l, new C(this)), this.consumeContext(E, (e) => {
      this.observe(
        e?.value,
        async (t) => {
          const a = this.alias && typeof t == "object" ? t[this.alias] : t, r = x(a);
          if (!r.length) {
            this.value = "";
            return;
          }
          const s = await Promise.all(r.map((i) => h(this, n, v).call(this, i)));
          this.value = s.filter((i) => i).join(", ");
        },
        "observeValue"
      );
    });
  }
};
l = /* @__PURE__ */ new WeakMap();
n = /* @__PURE__ */ new WeakSet();
v = async function(e) {
  return await h(this, n, _).call(this, e) || e?.name || D(e, e?.url) || "";
};
_ = async function(e) {
  if (e?.unique)
    return P(this, l).names(e.type, [e.unique]);
};
f([
  d()
], p.prototype, "alias", 2);
p = f([
  y("ufm-link-display")
], p);
class B extends U {
  render(t) {
    return t.text ? `<ufm-link-display ${super.getAttributes(t.text)}></ufm-link-display>` : void 0;
  }
}
export {
  B as UfmLinkDisplayComponent,
  p as UfmLinkDisplayElement,
  B as api
};
//# sourceMappingURL=link-display.component-BDj3YWks.js.map
