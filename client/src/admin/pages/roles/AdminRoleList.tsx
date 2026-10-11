import React, { useEffect, useState } from 'react';
import { Plus, Trash2, Edit, ShieldCheck } from 'lucide-react';
import { adminService, RoleDto } from '../../services/adminService';
import toast from 'react-hot-toast';

export const AdminRoleList: React.FC = () => {
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [allPermissions, setAllPermissions] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);

  // Role Form Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [roleName, setRoleName] = useState('');
  const [description, setDescription] = useState('');
  const [selectedPermissions, setSelectedPermissions] = useState<string[]>([]);
  const [saving, setSaving] = useState(false);

  const fetchRolesAndPermissions = async () => {
    setLoading(true);
    try {
      const [r, p] = await Promise.all([
        adminService.getRoles(),
        adminService.getAllPermissions().catch(() => [
          'product.view', 'product.create', 'product.update', 'product.delete',
          'category.view', 'category.create', 'category.update', 'category.delete',
          'order.view', 'order.manage',
          'user.view', 'user.manage',
          'role.view', 'role.manage',
          'coupon.view', 'coupon.manage',
          'review.moderate', 'banner.manage', 'shipping.manage', 'return.manage',
          'audit.view', 'stock.view', 'statistics.view'
        ]),
      ]);
      setRoles(r);
      setAllPermissions(p);
    } catch {
      toast.error('Rollar yüklənə bilmədi.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRolesAndPermissions();
  }, []);

  const handleOpenCreate = () => {
    setEditingId(null);
    setRoleName('');
    setDescription('');
    setSelectedPermissions([]);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (role: RoleDto) => {
    setEditingId(role.id);
    setRoleName(role.name);
    setDescription(role.description || '');
    setSelectedPermissions([...(role.permissions || [])]);
    setIsModalOpen(true);
  };

  const togglePermission = (perm: string) => {
    setSelectedPermissions((prev) =>
      prev.includes(perm) ? prev.filter((p) => p !== perm) : [...prev, perm]
    );
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      if (editingId) {
        await adminService.updateRole(editingId, {
          name: roleName,
          description,
          permissions: selectedPermissions,
        });
        toast.success('Rol uğurla yeniləndi!');
      } else {
        await adminService.createRole({
          name: roleName,
          description,
          permissions: selectedPermissions,
        });
        toast.success('Yeni rol yaradıldı!');
      }
      setIsModalOpen(false);
      fetchRolesAndPermissions();
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Rol saxlanılarkən xəta baş verdi.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`"${name}" rolunu silmək istədiyinizdən əminsiniz?`)) return;
    try {
      await adminService.deleteRole(id);
      toast.success('Rol silindi.');
      fetchRolesAndPermissions();
    } catch {
      toast.error('Rol silinərkən xəta baş verdi.');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">
            Rollar & İcazələr (RBAC)
          </h1>
          <p className="text-xs text-gray-500 mt-1">İstifadəçi hüquqları və icazə qrupları</p>
        </div>

        <button
          onClick={handleOpenCreate}
          className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold flex items-center gap-1.5 shadow-md shadow-indigo-600/30 transition"
        >
          <Plus className="w-4 h-4" /> Yeni Rol
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {roles.map((role) => (
          <div
            key={role.id}
            className="p-6 rounded-3xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 shadow-sm space-y-4 flex flex-col justify-between"
          >
            <div>
              <div className="flex items-center justify-between mb-1">
                <h3 className="font-bold text-base text-gray-900 dark:text-gray-100">
                  {role.name}
                </h3>
                <span className="text-[10px] font-mono px-2 py-0.5 rounded-full bg-indigo-50 text-indigo-700 dark:bg-indigo-950/60 dark:text-indigo-400 font-bold">
                  {role.permissions?.length || 0} icazə
                </span>
              </div>
              <p className="text-xs text-gray-500">{role.description || 'Təsvir qeyd edilməyib'}</p>

              <div className="mt-4 flex flex-wrap gap-1 max-h-24 overflow-y-auto">
                {role.permissions?.map((p, i) => (
                  <span
                    key={i}
                    className="text-[10px] font-mono px-2 py-0.5 rounded bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400"
                  >
                    {p}
                  </span>
                ))}
              </div>
            </div>

            <div className="flex items-center justify-end gap-2 pt-4 border-t border-gray-100 dark:border-gray-800">
              <button
                onClick={() => handleOpenEdit(role)}
                className="p-1.5 text-indigo-600 hover:bg-indigo-50 rounded-lg transition"
              >
                <Edit className="w-4 h-4" />
              </button>
              <button
                onClick={() => handleDelete(role.id, role.name)}
                className="p-1.5 text-rose-500 hover:bg-rose-50 rounded-lg transition"
              >
                <Trash2 className="w-4 h-4" />
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* Role Create/Edit Modal with Permission Checkboxes */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <form
            onSubmit={handleSave}
            className="w-full max-w-2xl bg-white dark:bg-gray-800 rounded-3xl p-6 shadow-2xl space-y-4 max-h-[90vh] flex flex-col"
          >
            <h3 className="font-bold text-base text-gray-900 dark:text-gray-100">
              {editingId ? 'Rolu Redaktə Et' : 'Yeni Rol Yarat'}
            </h3>

            <div className="space-y-3">
              <div>
                <label className="block text-xs font-semibold mb-1">Rolun Adı *</label>
                <input
                  type="text"
                  required
                  placeholder="Məsələn: Moderator"
                  value={roleName}
                  onChange={(e) => setRoleName(e.target.value)}
                  className="w-full px-4 py-2 text-xs rounded-xl border bg-gray-50 dark:bg-gray-900"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold mb-1">Təsvir</label>
                <input
                  type="text"
                  placeholder="Rolun vəzifə öhdəlikləri haqqında..."
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  className="w-full px-4 py-2 text-xs rounded-xl border bg-gray-50 dark:bg-gray-900"
                />
              </div>
            </div>

            {/* Permissions Checkbox Grid */}
            <div className="flex-1 overflow-y-auto pt-2 space-y-2 border-t border-gray-100 dark:border-gray-700">
              <label className="block text-xs font-bold uppercase tracking-wider text-gray-500">
                İcazələr ({selectedPermissions.length} seçilib)
              </label>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                {allPermissions.map((perm) => {
                  const checked = selectedPermissions.includes(perm);
                  return (
                    <label
                      key={perm}
                      className={`flex items-center gap-2 p-2 rounded-xl border text-xs cursor-pointer transition ${
                        checked
                          ? 'border-indigo-600 bg-indigo-50/50 dark:bg-indigo-950/40 text-indigo-700 dark:text-indigo-300 font-semibold'
                          : 'border-gray-200 dark:border-gray-700 text-gray-600 dark:text-gray-400'
                      }`}
                    >
                      <input
                        type="checkbox"
                        checked={checked}
                        onChange={() => togglePermission(perm)}
                        className="rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                      />
                      <span className="font-mono text-[11px]">{perm}</span>
                    </label>
                  );
                })}
              </div>
            </div>

            <div className="flex gap-2 pt-4 border-t border-gray-100 dark:border-gray-700">
              <button
                type="button"
                onClick={() => setIsModalOpen(false)}
                className="flex-1 py-2 text-xs rounded-xl border font-semibold"
              >
                Ləğv et
              </button>
              <button
                type="submit"
                disabled={saving}
                className="flex-1 py-2 text-xs rounded-xl bg-indigo-600 text-white font-bold disabled:opacity-50"
              >
                {saving ? 'Yadda saxlanılır...' : 'Rolu Yadda Saxla'}
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
};
