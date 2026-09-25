const n = /^umb:\/\/(document|media)\/([0-9a-fA-F]{32})$/;
function o(t) {
  if (!t) return [];
  let e = t;
  if (typeof e == "string")
    try {
      e = JSON.parse(e);
    } catch {
      return [];
    }
  return (Array.isArray(e) ? e : [e]).filter((r) => !!r && typeof r == "object").map(s);
}
function a(t, e) {
  return `${e ?? ""}${t.queryString ?? ""}` || void 0;
}
function u(t) {
  const e = n.exec(t);
  if (!e) return;
  const [, i, r] = e;
  return { type: i, unique: c(r) };
}
function s(t) {
  if (t.unique || !t.udi) return t;
  const e = u(t.udi);
  return e ? { ...t, type: t.type ?? e.type, unique: e.unique } : t;
}
function c(t) {
  return [t.slice(0, 8), t.slice(8, 12), t.slice(12, 16), t.slice(16, 20), t.slice(20)].join("-").toLowerCase();
}
export {
  u as a,
  o as p,
  a as w
};
//# sourceMappingURL=link-value.function-D3PQjIif.js.map
