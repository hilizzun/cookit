import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { Link } from "react-router-dom";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { Card, CardContent } from "../components/ui/card";

export default function LoginPage() {
  const [form, setForm] = useState({ email: "", password: "" });
  const [error, setError] = useState("");
  const navigate = useNavigate();
  const { login, accessToken, isBlocked, user } = useAuth();

  useEffect(() => {
    if (accessToken && (isBlocked || user?.isBlocked)) {
      navigate('/blocked');
    } else if (accessToken) {
      navigate('/');
    }
  }, [accessToken, isBlocked, user, navigate]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    try {
      const result = await login(form);
      if (result?.isBlocked) return;
      navigate("/");
    } catch (err) {
      console.error("Login error:", err);
      if (err.message.includes("заблокирован") || err.message.includes("blocked")) {
        setError(err.message);
      } else if (err.message.includes("Подтвердите") || err.message.includes("email")) {
        setError(
          <span>
            {err.message}.{" "}
            <button
              type="button"
              onClick={() => navigate(`/resend-verification?email=${form.email}`)}
              className="underline font-medium text-primary"
            >
              Отправить письмо повторно
            </button>
          </span>
        );
      } else if (err.message.includes("JSON")) {
        setError("Пользователь с такой почтой ещё не зарегистрирован");
      } else {
        setError("Ошибка входа. Попробуйте ещё раз!");
      }
    }
  };

  return (
    <div className="flex flex-col justify-center min-h-screen p-4 bg-white">
      <Card className="max-w-md w-full mx-auto border-0 shadow-sm">
        <CardContent className="p-8">
          <div className="flex justify-center mb-8">
            <Link to="/">
              <img src="/logo.svg" alt="Logo" className="h-15" />
            </Link>
          </div>
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="space-y-2">
              <Label htmlFor="email">Почта</Label>
              <Input
                id="email"
                name="email"
                type="email"
                value={form.email}
                onChange={handleChange}
                placeholder="Введите адрес электронной почты"
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Пароль</Label>
              <Input
                id="password"
                name="password"
                type="password"
                value={form.password}
                onChange={handleChange}
                placeholder="Введите пароль"
                required
              />
            </div>
            {error && (
              <div className="text-sm text-destructive text-center">{error}</div>
            )}
            <Button type="submit" variant="violet" className="w-full">
              Войти
            </Button>
            <p className="text-sm text-center text-muted-foreground">
              Ещё нет аккаунта?{" "}
              <Link to="/register" className="font-medium text-primary hover:underline">
                Зарегистрироваться здесь
              </Link>
            </p>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}