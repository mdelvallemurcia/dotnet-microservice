import { Input } from "../components/Input";

export const LoginPage = () => {
    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-100 px-4">
            <div className="max-w-md w-full bg-white rounded-2xl shadow-xl p-8">
                <h2 className="text-3xl font-extrabold text-center text-gray-800 mb-8">Bienvenido</h2>
                <form>
                    <Input label="Email" type="email" placeholder="tu@email.com" />
                    <Input label="Password" type="password" placeholder="••••••••" />
                    <button className="w-full bg-blue-600 hover:bg-blue-700 text-white font-bold py-3 rounded-lg transition-colors mt-4">
                        Iniciar Sesión
                    </button>
                </form>
                <p className="text-center text-sm text-gray-600 mt-6">
                    ¿No tienes cuenta? <a href="#" className="text-blue-500 hover:underline">Regístrate</a>
                </p>
            </div>
        </div>
    );
};