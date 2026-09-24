const n = /^umb:\/\/(document|media)\/([0-9a-fA-F]{32})$/;
function u(t) {
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
function s(t) {
  if (t.unique || !t.udi) return t;
  const e = n.exec(t.udi);
  if (!e) return t;
  const [, i, r] = e;
  return { ...t, type: t.type ?? i, unique: c(r) };
}
function c(t) {
  return [t.slice(0, 8), t.slice(8, 12), t.slice(12, 16), t.slice(16, 20), t.slice(20)].join("-").toLowerCase();
}
export {
  u as p
};
//# sourceMappingURL=link-value.function-DaOoVRsf.js.map
