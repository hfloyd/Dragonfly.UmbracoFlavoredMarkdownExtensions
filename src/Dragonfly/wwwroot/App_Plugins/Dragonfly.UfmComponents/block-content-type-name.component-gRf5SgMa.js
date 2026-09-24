import { property as T, state as d, customElement as E } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as O } from "@umbraco-cms/backoffice/lit-element";
import { UmbUfmComponentBase as k } from "@umbraco-cms/backoffice/ufm";
import { UMB_BLOCK_ENTRY_CONTEXT as b, UMB_BLOCK_MANAGER_CONTEXT as g, UMB_BLOCK_WORKSPACE_CONTEXT as w } from "@umbraco-cms/backoffice/block";
var B = Object.defineProperty, N = Object.getOwnPropertyDescriptor, u = (e) => {
  throw TypeError(e);
}, h = (e, t, n, o) => {
  for (var r = o > 1 ? void 0 : o ? N(t, n) : t, m = e.length - 1, l; m >= 0; m--)
    (l = e[m]) && (r = (o ? l(t, n, r) : l(r)) || r);
  return o && r && B(t, n, r), r;
}, v = (e, t, n) => t.has(e) || u("Cannot " + n), s = (e, t, n) => (v(e, t, "read from private field"), t.get(e)), _ = (e, t, n) => t.has(e) ? u("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, n), C = (e, t, n, o) => (v(e, t, "write to private field"), t.set(e, n), n), y = (e, t, n) => (v(e, t, "access private method"), n), a, p, i, f;
let c = class extends O {
  constructor() {
    super(), _(this, i), _(this, a), _(this, p), this.consumeContext(b, (e) => {
      e && this.observe(
        e.contentElementTypeKey,
        (t) => {
          C(this, p, t), y(this, i, f).call(this);
        },
        "_observeContentElementTypeKey"
      );
    }), this.consumeContext(g, (e) => {
      C(this, a, e), y(this, i, f).call(this);
    }), this.consumeContext(w, (e) => {
      e && this.observe(
        e.content.structure.ownerContentTypeObservablePart((t) => t?.name),
        (t) => {
          t && (this._name = t);
        },
        "_observeOwnerContentTypeName"
      );
    });
  }
  render() {
    return this._name ?? this.fallback ?? "";
  }
};
a = /* @__PURE__ */ new WeakMap();
p = /* @__PURE__ */ new WeakMap();
i = /* @__PURE__ */ new WeakSet();
f = async function() {
  if (!s(this, a) || !s(this, p)) return;
  await s(this, a).contentTypesLoaded;
  const e = s(this, a).getContentTypeNameOf(s(this, p));
  e && (this._name = e);
};
h([
  T()
], c.prototype, "fallback", 2);
h([
  d()
], c.prototype, "_name", 2);
c = h([
  E("ufm-block-content-type-name")
], c);
class x extends k {
  render(t) {
    return `<ufm-block-content-type-name fallback="${(t.text ?? "").replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;").replace(/>/g, "&gt;")}"></ufm-block-content-type-name>`;
  }
}
export {
  x as UfmBlockContentTypeNameComponent,
  c as UfmBlockContentTypeNameElement,
  x as api
};
//# sourceMappingURL=block-content-type-name.component-gRf5SgMa.js.map
