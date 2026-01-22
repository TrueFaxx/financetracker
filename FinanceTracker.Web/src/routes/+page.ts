import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch, url }) => {
  const month = url.searchParams.get('month') ?? new Date().toISOString().slice(0, 7);

  const [monthlyRes, topRes, biggestRes] = await Promise.all([
    fetch('/api/summary/monthly?months=6'),
    fetch(`/api/top/merchants?month=${month}`),
    fetch(`/api/biggest?month=${month}`)
  ]);

  return {
    month,
    monthly: await monthlyRes.json(),
    top: await topRes.json(),
    biggest: await biggestRes.json()
  };
};
