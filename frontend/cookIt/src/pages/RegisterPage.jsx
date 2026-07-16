import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { registerUser } from "../services/authService";
import { useAuth } from "../contexts/AuthContext";
import { Link } from "react-router-dom";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { Card, CardContent } from "../components/ui/card";
import { CheckCircle, XCircle } from "lucide-react";

export default function RegisterPage() {
  const [form, setForm] = useState({
    username: "",
    fullname: "",
    email: "",
    password: "",
    confirm: "",
  });

  const [error, setError] = useState("");
  const [passwordChecks, setPasswordChecks] = useState({
    length: false,
    digit: false,
    lowercase: false,
    uppercase: false,
    special: false,
  });
  const [passwordMatch, setPasswordMatch] = useState(false);
  const navigate = useNavigate();
  const { login } = useAuth();

  useEffect(() => {
    setPasswordChecks({
      length: form.password.length >= 8,
      digit: /\d/.test(form.password),
      lowercase: /[a-z]/.test(form.password),
      uppercase: /[A-Z]/.test(form.password),
      special: /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(form.password),
    });
    setPasswordMatch(form.password === form.confirm && form.password !== "");
  }, [form.password, form.confirm]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (!form.username || !form.fullname || !form.email || !form.password || !form.confirm) {
      setError("Все поля должны быть заполнены");
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(form.email)) {
      setError("Неверный формат почты");
      return;
    }

    const allChecksPassed = Object.values(passwordChecks).every(Boolean);
    if (!allChecksPassed) {
      setError("Пароль не соответствует требованиям");
      return;
    }

    if (!passwordMatch) {
      setError("Пароли не совпадают");
      return;
    }

    try {
      await registerUser(form);
      navigate("/email-verification-sent", { state: { email: form.email } });
    } catch (err) {
      if (err.message.includes("Пользователь")) {
        setError("Пользователь с таким ником уже существует");
      } else if (err.message.includes("Email")) {
        setError("Пользователь с таким адресом почты уже существует");
      } else {
        setError(err.message || "Ошибка регистрации");
      }
    }
  };

  const PasswordRequirement = ({ text, isValid }) => (
    <div className={`flex items-center text-xs ${isValid ? "text-green-600" : "text-muted-foreground"}`}>
      {isValid ? (
        <CheckCircle className="w-4 h-4 mr-1 text-green-600" />
      ) : (
        <XCircle className="w-4 h-4 mr-1 text-muted-foreground" />
      )}
      {text}
    </div>
  );

  return (
    <div className="flex flex-col justify-center min-h-screen p-4 overflow-y-auto bg-white">
      <Card className="max-w-md w-full mx-auto border-0 shadow-sm">
        <CardContent className="p-8">
          <div className="flex justify-center mb-8">
            <Link to="/">
              <img src="/logo.svg" alt="Logo" className="h-15" />
            </Link>
          </div>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="username">Ник</Label>
              <Input
                id="username"
                name="username"
                value={form.username}
                onChange={handleChange}
                placeholder="Введите ник"
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="fullname">Фамилия Имя</Label>
              <Input
                id="fullname"
                name="fullname"
                value={form.fullname}
                onChange={handleChange}
                placeholder="Введите фамилию и имя"
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="email">Электронная почта</Label>
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
              <div className="mt-2 space-y-1">
                <PasswordRequirement text="Минимум 8 символов" isValid={passwordChecks.length} />
                <PasswordRequirement text="Содержит цифру" isValid={passwordChecks.digit} />
                <PasswordRequirement text="Строчная буква (a-z)" isValid={passwordChecks.lowercase} />
                <PasswordRequirement text="Заглавная буква (A-Z)" isValid={passwordChecks.uppercase} />
                <PasswordRequirement text="Спецсимвол (!@#$%^&*)" isValid={passwordChecks.special} />
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="confirm">Подтверждение пароля</Label>
              <Input
                id="confirm"
                name="confirm"
                type="password"
                value={form.confirm}
                onChange={handleChange}
                placeholder="Подтвердите свой пароль"
                required
                className={form.confirm ? (passwordMatch ? "border-green-500" : "border-destructive") : ""}
              />
              {form.confirm && (
                <div className={`text-xs ${passwordMatch ? "text-green-600" : "text-destructive"}`}>
                  {passwordMatch ? "Пароли совпадают" : "Пароли не совпадают"}
                </div>
              )}
            </div>
            {error && (
              <div className="text-sm text-destructive text-center">{error}</div>
            )}
            <Button type="submit" variant="violet" className="w-full">
              Зарегистрироваться
            </Button>
            <p className="text-sm text-center text-muted-foreground">
              Уже есть аккаунт?{" "}
              <Link to="/login" className="font-medium text-primary hover:underline">
                Войти здесь
              </Link>
            </p>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}