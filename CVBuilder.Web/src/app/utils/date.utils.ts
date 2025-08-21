export function toLocalDate(iso: string | null | undefined): Date | null {
  if (!iso) return null;
  const hasTZ = /[zZ]|[+\-]\d{2}:\d{2}$/.test(iso);
  const normalized = hasTZ ? iso : iso + 'Z';
  return new Date(normalized);
}
