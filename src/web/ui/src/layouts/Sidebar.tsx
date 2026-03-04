import { NavLink } from 'react-router-dom';

interface SidebarProps {
    isCollapsed: boolean;
}

export const Sidebar = ({ isCollapsed }: SidebarProps) => {
    // Configuración de los enlaces del menú
    const menuItems = [
        { name: 'Inicio', path: '/home', icon: '🏠' },
        { name: 'Proyectos', path: '/projects', icon: '📓' },
        //{ name: 'Configuración', path: '/settings', icon: '⚙️' },
    ];

    return (
        <aside className={`${isCollapsed ? 'w-20' : 'w-64'} bg-slate-800 text-white transition-all duration-300 flex flex-col`}>
            <div className="p-4 font-bold text-xl border-b border-slate-700">
                {isCollapsed ? '🚀' : 'My App'}
            </div>

            <nav className="flex-1 p-4 space-y-2">
                {menuItems.map((item) => (
                    <NavLink
                        key={item.path}
                        to={item.path}
                        className={({ isActive }) => `
                          flex items-center gap-3 p-2 rounded-lg transition-colors
                          ${isActive
                                ? 'bg-blue-600 text-white'
                                : 'text-slate-300 hover:bg-slate-700 hover:text-white'}
                        `}
                    >
                        <span className="text-xl">{item.icon}</span>
                        {!isCollapsed && <span>{item.name}</span>}
                    </NavLink>
                ))}
            </nav>
        </aside>
    );
};