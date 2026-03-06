import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Input } from "../components/Input";
import { useAuth } from "../hooks/useAuth";
import { useFetch } from "../hooks/useFetch";

export const LoginPage = () => {
    const navigate = useNavigate();
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const { afterLoginActions } = useAuth();

    const { loading, error, refetch: executeLogin } = useFetch<{ accessToken: string }>(
        '/v1/login',
        { method: 'POST' }
    );

    const handleLogin = async (e: React.SyntheticEvent) => {
        e.preventDefault();
        const result = await executeLogin({
            body: { username, password }
        });

        if (result && result.accessToken) {
            afterLoginActions(result.accessToken);
            navigate('/home');
        }
    };

    const getFieldError = (fieldName: string): string | undefined => {
        if (error && typeof error === 'object' && 'errors' in error) {
            const messages = error.errors?.[fieldName];
            return Array.isArray(messages) ? messages[0] : undefined;
        }
        return undefined;
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-100 px-4">
            <div className="max-w-md w-full bg-white rounded-2xl shadow-xl p-8">
                <h2 className="text-3xl font-extrabold text-center text-gray-800 mb-8">Welcome</h2>

                {/* Mensaje de error general (si no es un error de validación de campos) */}
                {error && typeof error === 'string' && (
                    <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
                        {error}
                    </div>
                )}

                {/* Mensaje de error general si es ProblemDetails pero sin errores específicos */}
                {error && typeof error === 'object' && !error.errors && (
                    <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
                        {error.title || "Request error"}
                    </div>
                )}

                <form onSubmit={handleLogin}>
                    <div className="space-y-4">
                        <Input
                            label="Username"
                            type="text"
                            placeholder="Please, type your username"
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                            required
                            errorMessage={getFieldError('UserName') }
                        />

                        <Input
                            label="Password"
                            type="password"
                            placeholder="*********"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                            errorMessage={getFieldError('Password') }
                        />
                    </div>

                    <button
                        type="submit"
                        disabled={loading}
                        className={`w-full text-white font-bold py-3 rounded-lg transition-colors mt-4 ${loading ? 'bg-blue-300 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-700'
                            }`}
                    >
                        {loading ? 'Connecting...' : 'Login'}
                    </button>
                </form>
            </div>
        </div>
    );
};