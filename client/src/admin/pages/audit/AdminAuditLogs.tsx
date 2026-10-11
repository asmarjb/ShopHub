import React, { useEffect, useState } from 'react';
import { FileText, Search, ChevronLeft, ChevronRight } from 'lucide-react';
import { adminService, AuditLogDto } from '../../services/adminService';
import { PagedResult } from '../../../types';
import toast from 'react-hot-toast';

export const AdminAuditLogs: React.FC = () => {
  const [logsData, setLogsData] = useState<PagedResult<AuditLogDto> | null>(null);
  const [actionFilter, setActionFilter] = useState('');
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  const fetchLogs = async () => {
    setLoading(true);
    try {
      const data = await adminService.getAuditLogs({
        pageNumber: page,
        pageSize: 15,
        action: actionFilter || undefined,
      });
      setLogsData(data);
    } catch {
      toast.error('Audit jurnalı yüklənə bilmədi.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchLogs();
  }, [page, actionFilter]);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">Audit Jurnalı</h1>
        <p className="text-xs text-gray-500 mt-1">Sistemdə baş verən bütün mühüm əməliyyatların və dəyişikliklərin izi</p>
      </div>

      <div className="flex gap-3">
        <input
          type="text"
          placeholder="Əməliyyata görə filtr (məs: CREATE, UPDATE)..."
          value={actionFilter}
          onChange={(e) => {
            setActionFilter(e.target.value);
            setPage(1);
          }}
          className="px-4 py-2 text-xs rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800"
        />
      </div>

      <div className="bg-white dark:bg-gray-900 rounded-3xl border border-gray-200 dark:border-gray-800 overflow-hidden shadow-sm">
        <table className="w-full text-left text-xs">
          <thead className="bg-gray-50 dark:bg-gray-800/60 text-gray-500 uppercase text-[10px] tracking-wider border-b border-gray-200 dark:border-gray-800">
            <tr>
              <th className="px-6 py-4 font-bold">İstifadəçi</th>
              <th className="px-6 py-4 font-bold">Əməliyyat</th>
              <th className="px-6 py-4 font-bold">Obyekt (Entity)</th>
              <th className="px-6 py-4 font-bold">Tarix</th>
              <th className="px-6 py-4 font-bold">IP Ünvan</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
            {loading ? (
              <tr>
                <td colSpan={5} className="text-center py-10 text-gray-400">Yüklənir...</td>
              </tr>
            ) : logsData && logsData.items.length > 0 ? (
              logsData.items.map((log) => (
                <tr key={log.id} className="hover:bg-gray-50/50 dark:hover:bg-gray-800/30">
                  <td className="px-6 py-4 font-bold text-gray-900 dark:text-gray-100">
                    {log.userName || log.userId || 'Sistem'}
                  </td>
                  <td className="px-6 py-4">
                    <span className="font-mono px-2 py-0.5 rounded bg-indigo-50 text-indigo-700 dark:bg-indigo-950/60 dark:text-indigo-400 font-bold text-[10px]">
                      {log.action}
                    </span>
                  </td>
                  <td className="px-6 py-4 font-semibold text-gray-700 dark:text-gray-300">
                    {log.entityName} {log.entityId ? `(#${log.entityId.slice(0, 8)})` : ''}
                  </td>
                  <td className="px-6 py-4 text-gray-400">
                    {new Date(log.createdAt).toLocaleString()}
                  </td>
                  <td className="px-6 py-4 text-gray-400 font-mono text-[11px]">
                    {log.ipAddress || '-'}
                  </td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan={5} className="text-center py-10 text-gray-400">Jurnalda qeyd tapılmadı.</td>
              </tr>
            )}
          </tbody>
        </table>

        {/* Pagination */}
        {logsData && logsData.totalPages > 1 && (
          <div className="p-4 border-t border-gray-100 dark:border-gray-800 flex items-center justify-between text-xs text-gray-500">
            <span>Səhifə {logsData.pageNumber} / {logsData.totalPages}</span>
            <div className="flex gap-2">
              <button
                disabled={!logsData.hasPreviousPage}
                onClick={() => setPage((p) => p - 1)}
                className="p-1.5 border rounded-lg disabled:opacity-40"
              >
                <ChevronLeft className="w-4 h-4" />
              </button>
              <button
                disabled={!logsData.hasNextPage}
                onClick={() => setPage((p) => p + 1)}
                className="p-1.5 border rounded-lg disabled:opacity-40"
              >
                <ChevronRight className="w-4 h-4" />
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
