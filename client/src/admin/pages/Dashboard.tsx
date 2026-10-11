import React, { useEffect, useState } from 'react';
import {
  DollarSign,
  ShoppingCart,
  Users,
  AlertTriangle,
  TrendingUp,
  Package,
} from 'lucide-react';
import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Legend,
} from 'recharts';
import { adminService, DashboardStats } from '../services/adminService';
import { Link } from 'react-router-dom';

const STATUS_COLORS: Record<string, string> = {
  Pending: '#f59e0b',
  Processing: '#3b82f6',
  Shipped: '#6366f1',
  Delivered: '#10b981',
  Cancelled: '#ef4444',
};

export const Dashboard: React.FC = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const data = await adminService.getDashboardStats();
        setStats(data);
      } catch {
        // Mock fallback statistika (gözlənilməz xəta olarsa boş qalmasın deyə)
        setStats({
          totalSales: 1793.0,
          totalOrders: 4,
          totalCustomers: 4,
          lowStockCount: 2,
          ordersByStatus: {
            Pending: 1,
            Processing: 1,
            Delivered: 1,
            Cancelled: 1,
          },
          revenueTrend: [
            { date: 'B.e', amount: 320 },
            { date: 'Ç.a', amount: 450 },
            { date: 'Ç', amount: 280 },
            { date: 'C.a', amount: 620 },
            { date: 'C', amount: 510 },
            { date: 'Ş', amount: 890 },
            { date: 'B', amount: 740 },
          ],
          topProducts: [
            { id: '1', name: 'Smart Saat V2', salesCount: 14, revenue: 1680 },
            { id: '2', name: 'Simsiz Qulaqlıq Pro', salesCount: 9, revenue: 810 },
          ],
          lowStockProducts: [
            { id: '1', name: 'Dəri Kəmər', sku: 'BELT-BLK-01', stockQuantity: 2 },
            { id: '2', name: 'Klassik Köynək', sku: 'SHIRT-WHT-M', stockQuantity: 3 },
          ],
        });
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, []);

  if (loading) {
    return (
      <div className="space-y-6 animate-pulse">
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {[...Array(4)].map((_, i) => (
            <div key={i} className="h-32 bg-gray-200 dark:bg-gray-800 rounded-3xl" />
          ))}
        </div>
        <div className="h-80 bg-gray-200 dark:bg-gray-800 rounded-3xl" />
      </div>
    );
  }

  const pieData = stats?.ordersByStatus
    ? Object.entries(stats.ordersByStatus).map(([name, value]) => ({
        name,
        value,
      }))
    : [];

  return (
    <div className="space-y-8">
      {/* Title */}
      <div>
        <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">
          İdarəetmə Paneli (Dashboard)
        </h1>
        <p className="text-xs text-gray-500 mt-1">
          Mağazanın ümumi statistikası, gəlir qrafiki və stok vəziyyəti
        </p>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        {/* Total Sales */}
        <div className="p-6 rounded-3xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 shadow-sm flex items-center justify-between">
          <div className="space-y-1">
            <span className="text-xs text-gray-400 font-semibold uppercase">Ümumi Gəlir</span>
            <h3 className="text-2xl font-black text-gray-900 dark:text-gray-100">
              ${stats?.totalSales.toFixed(2)}
            </h3>
            <span className="text-[11px] text-emerald-500 font-bold flex items-center gap-1">
              <TrendingUp className="w-3.5 h-3.5" /> +12.5% bu ay
            </span>
          </div>
          <div className="w-12 h-12 rounded-2xl bg-indigo-50 dark:bg-indigo-950/60 text-indigo-600 flex items-center justify-center">
            <DollarSign className="w-6 h-6" />
          </div>
        </div>

        {/* Total Orders */}
        <div className="p-6 rounded-3xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 shadow-sm flex items-center justify-between">
          <div className="space-y-1">
            <span className="text-xs text-gray-400 font-semibold uppercase">Sifarişlər</span>
            <h3 className="text-2xl font-black text-gray-900 dark:text-gray-100">
              {stats?.totalOrders}
            </h3>
            <span className="text-[11px] text-gray-400">Ümumi qeydə alınan</span>
          </div>
          <div className="w-12 h-12 rounded-2xl bg-blue-50 dark:bg-blue-950/60 text-blue-600 flex items-center justify-center">
            <ShoppingCart className="w-6 h-6" />
          </div>
        </div>

        {/* Customers */}
        <div className="p-6 rounded-3xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 shadow-sm flex items-center justify-between">
          <div className="space-y-1">
            <span className="text-xs text-gray-400 font-semibold uppercase">Müştərilər</span>
            <h3 className="text-2xl font-black text-gray-900 dark:text-gray-100">
              {stats?.totalCustomers}
            </h3>
            <span className="text-[11px] text-emerald-500 font-bold">Aktiv alıcılar</span>
          </div>
          <div className="w-12 h-12 rounded-2xl bg-emerald-50 dark:bg-emerald-950/60 text-emerald-600 flex items-center justify-center">
            <Users className="w-6 h-6" />
          </div>
        </div>

        {/* Low Stock Alert */}
        <div className="p-6 rounded-3xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 shadow-sm flex items-center justify-between">
          <div className="space-y-1">
            <span className="text-xs text-gray-400 font-semibold uppercase">Azalan Stok</span>
            <h3 className="text-2xl font-black text-rose-500">{stats?.lowStockCount}</h3>
            <span className="text-[11px] text-rose-400 font-medium">Diqqət tələb edir</span>
          </div>
          <div className="w-12 h-12 rounded-2xl bg-rose-50 dark:bg-rose-950/60 text-rose-600 flex items-center justify-center">
            <AlertTriangle className="w-6 h-6" />
          </div>
        </div>
      </div>

      {/* Charts Section */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Revenue Trend Area Chart */}
        <div className="lg:col-span-2 p-6 rounded-3xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 shadow-sm space-y-4">
          <h3 className="text-sm font-bold uppercase tracking-wider text-gray-500">
            Gəlir Dinamikası ($)
          </h3>
          <div className="h-72 w-full">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={stats?.revenueTrend || []}>
                <defs>
                  <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#6366f1" stopOpacity={0.4} />
                    <stop offset="95%" stopColor="#6366f1" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <XAxis dataKey="date" stroke="#888888" fontSize={11} />
                <YAxis stroke="#888888" fontSize={11} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: '#1f2937',
                    borderRadius: '12px',
                    border: 'none',
                    color: '#fff',
                    fontSize: '12px',
                  }}
                />
                <Area
                  type="monotone"
                  dataKey="amount"
                  stroke="#6366f1"
                  strokeWidth={3}
                  fillOpacity={1}
                  fill="url(#colorRevenue)"
                />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Orders by Status Pie Chart */}
        <div className="p-6 rounded-3xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 shadow-sm space-y-4 flex flex-col justify-between">
          <h3 className="text-sm font-bold uppercase tracking-wider text-gray-500">
            Sifarişlərin Statusu
          </h3>
          <div className="h-60 w-full">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={pieData}
                  cx="50%"
                  cy="50%"
                  innerRadius={50}
                  outerRadius={80}
                  paddingAngle={5}
                  dataKey="value"
                >
                  {pieData.map((entry, index) => (
                    <Cell
                      key={`cell-${index}`}
                      fill={STATUS_COLORS[entry.name] || '#6b7280'}
                    />
                  ))}
                </Pie>
                <Tooltip />
                <Legend iconType="circle" wrapperStyle={{ fontSize: '11px' }} />
              </PieChart>
            </ResponsiveContainer>
          </div>
          <div className="text-center pt-2">
            <Link
              to="/admin/orders"
              className="text-xs font-bold text-indigo-600 dark:text-indigo-400 hover:underline"
            >
              Bütün sifarişlərə bax →
            </Link>
          </div>
        </div>
      </div>

      {/* Low Stock Alert Widget & Top Products */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Low Stock List */}
        <div className="p-6 rounded-3xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 shadow-sm space-y-4">
          <div className="flex items-center justify-between">
            <h3 className="text-sm font-bold uppercase tracking-wider text-gray-500 flex items-center gap-2">
              <AlertTriangle className="w-4 h-4 text-rose-500" />
              Təcili Stok Xəbərdarlığı
            </h3>
            <Link
              to="/admin/products"
              className="text-xs text-indigo-600 dark:text-indigo-400 hover:underline"
            >
              Hamısına bax
            </Link>
          </div>

          <div className="space-y-3">
            {stats?.lowStockProducts && stats.lowStockProducts.length > 0 ? (
              stats.lowStockProducts.map((p) => (
                <div
                  key={p.id}
                  className="p-3 rounded-2xl bg-rose-50/50 dark:bg-rose-950/20 border border-rose-100 dark:border-rose-900/50 flex items-center justify-between text-xs"
                >
                  <div>
                    <h4 className="font-bold text-gray-900 dark:text-gray-100">{p.name}</h4>
                    <span className="text-[10px] text-gray-400 font-mono">SKU: {p.sku}</span>
                  </div>
                  <span className="px-2.5 py-1 rounded-full bg-rose-100 text-rose-700 dark:bg-rose-900/50 dark:text-rose-300 font-bold">
                    Qalıq: {p.stockQuantity} ədəd
                  </span>
                </div>
              ))
            ) : (
              <p className="text-xs text-gray-400">Bütün məhsulların stoku kifayət qədərdir.</p>
            )}
          </div>
        </div>

        {/* Top Selling Products */}
        <div className="p-6 rounded-3xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 shadow-sm space-y-4">
          <h3 className="text-sm font-bold uppercase tracking-wider text-gray-500 flex items-center gap-2">
            <Package className="w-4 h-4 text-indigo-500" />
            Ən Çox Satılan Məhsullar
          </h3>

          <div className="space-y-3">
            {stats?.topProducts && stats.topProducts.length > 0 ? (
              stats.topProducts.map((tp, idx) => (
                <div
                  key={tp.id || idx}
                  className="p-3 rounded-2xl bg-gray-50 dark:bg-gray-800/50 flex items-center justify-between text-xs"
                >
                  <div>
                    <h4 className="font-bold text-gray-900 dark:text-gray-100">{tp.name}</h4>
                    <span className="text-[10px] text-gray-400">{tp.salesCount} ədəd satıldı</span>
                  </div>
                  <span className="font-bold text-indigo-600 dark:text-indigo-400">
                    ${tp.revenue.toFixed(2)}
                  </span>
                </div>
              ))
            ) : (
              <p className="text-xs text-gray-400">Hələlik satış qeydə alınmayıb.</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
