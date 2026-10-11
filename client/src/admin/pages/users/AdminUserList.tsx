import React, { useEffect, useState } from 'react';
import { Users, Shield, Edit } from 'lucide-react';
import { adminService, AdminUserDto, RoleDto } from '../../services/adminService';
import toast from 'react-hot-toast';

export const AdminUserList: React.FC = () => {
  const [users, setUsers] = useState<AdminUserDto[]>([]);
  const [availableRoles, setAvailableRoles] = useState<RoleDto[]>([]);
  const [loading, setLoading] = useState(true);

  // Role Assignment Modal State
  const [selectedUser, setSelectedUser] = useState<AdminUserDto | null>(null);
  const [userRoles, setUserRoles] = useState<string[]>([]);
  const [saving, setSaving] = useState(false);

  const fetchUsersAndRoles = async () => {
    setLoading(true);
    try {
      const [usersRes, rolesRes] = await Promise.all([
        adminService.getUsers(),
        adminService.getRoles(),
      ]);
      setUsers(usersRes.items || []);
      setAvailableRoles(rolesRes || []);
    } catch {
      toast.error('İstifadəçilər yüklənə bilmədi.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsersAndRoles();
  }, []);

  const handleOpenAssignModal = (u: AdminUserDto) => {
    setSelectedUser(u);
    setUserRoles([...(u.roles || [])]);
  };

  const handleToggleRole = (roleName: string) => {
    setUserRoles((prev) =>
      prev.includes(roleName) ? prev.filter((r) => r !== roleName) : [...prev, roleName]
    );
  };

  const handleSaveRoles = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedUser) return;

    setSaving(true);
    try {
      await adminService.assignUserRoles(selectedUser.id, userRoles);
      toast.success(`${selectedUser.email} üçün rollar yeniləndi!`);
      setSelectedUser(null);
      fetchUsersAndRoles();
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Rollar təyin edilərkən xəta baş verdi.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">İstifadəçilər</h1>
        <p className="text-xs text-gray-500 mt-1">İstifadəçi hesabları və təyin edilmiş rollar</p>
      </div>

      <div className="bg-white dark:bg-gray-900 rounded-3xl border border-gray-200 dark:border-gray-800 overflow-hidden shadow-sm">
        <table className="w-full text-left text-xs">
          <thead className="bg-gray-50 dark:bg-gray-800/60 text-gray-500 uppercase text-[10px] tracking-wider border-b border-gray-200 dark:border-gray-800">
            <tr>
              <th className="px-6 py-4 font-bold">İstifadəçi</th>
              <th className="px-6 py-4 font-bold">E-poçt</th>
              <th className="px-6 py-4 font-bold">Rollar</th>
              <th className="px-6 py-4 font-bold">Status</th>
              <th className="px-6 py-4 font-bold text-right">Rolların İdarəsi</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
            {users.map((u) => (
              <tr key={u.id} className="hover:bg-gray-50/50 dark:hover:bg-gray-800/30">
                <td className="px-6 py-4 font-bold text-gray-900 dark:text-gray-100">
                  {u.firstName} {u.lastName}
                </td>
                <td className="px-6 py-4 text-gray-600 dark:text-gray-300 font-mono">{u.email}</td>
                <td className="px-6 py-4">
                  <div className="flex flex-wrap gap-1">
                    {u.roles?.map((r, i) => (
                      <span
                        key={i}
                        className="px-2 py-0.5 rounded-md bg-indigo-50 text-indigo-700 dark:bg-indigo-950/60 dark:text-indigo-400 font-bold text-[10px]"
                      >
                        {r}
                      </span>
                    ))}
                  </div>
                </td>
                <td className="px-6 py-4">
                  <span
                    className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                      u.emailConfirmed
                        ? 'bg-emerald-100 text-emerald-800 dark:bg-emerald-950/60 dark:text-emerald-400'
                        : 'bg-amber-100 text-amber-800 dark:bg-amber-950/60 dark:text-amber-400'
                    }`}
                  >
                    {u.emailConfirmed ? 'Təsdiqlənib' : 'Gözləyir'}
                  </span>
                </td>
                <td className="px-6 py-4 text-right">
                  <button
                    onClick={() => handleOpenAssignModal(u)}
                    className="px-3 py-1.5 rounded-xl bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 font-bold hover:bg-indigo-50 hover:text-indigo-600 transition inline-flex items-center gap-1.5"
                  >
                    <Shield className="w-3.5 h-3.5" /> Rollar
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Role Assignment Modal */}
      {selectedUser && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <form
            onSubmit={handleSaveRoles}
            className="w-full max-w-md bg-white dark:bg-gray-800 rounded-3xl p-6 shadow-2xl space-y-4"
          >
            <h3 className="font-bold text-base text-gray-900 dark:text-gray-100">
              Rolların Təyini: {selectedUser.email}
            </h3>

            <div className="space-y-2 py-2">
              {availableRoles.map((role) => {
                const checked = userRoles.includes(role.name);
                return (
                  <label
                    key={role.id}
                    className="flex items-center gap-3 p-3 rounded-xl border border-gray-200 dark:border-gray-700 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700/50"
                  >
                    <input
                      type="checkbox"
                      checked={checked}
                      onChange={() => handleToggleRole(role.name)}
                      className="rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                    />
                    <div>
                      <h4 className="font-bold text-xs text-gray-900 dark:text-gray-100">
                        {role.name}
                      </h4>
                      {role.description && (
                        <p className="text-[10px] text-gray-400">{role.description}</p>
                      )}
                    </div>
                  </label>
                );
              })}
            </div>

            <div className="flex gap-2 pt-2">
              <button
                type="button"
                onClick={() => setSelectedUser(null)}
                className="flex-1 py-2 text-xs rounded-xl border font-semibold"
              >
                Ləğv et
              </button>
              <button
                type="submit"
                disabled={saving}
                className="flex-1 py-2 text-xs rounded-xl bg-indigo-600 text-white font-bold disabled:opacity-50"
              >
                {saving ? 'Yadda saxlanılır...' : 'Rulları Yenilə'}
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
};
