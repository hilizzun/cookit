import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { resendVerification } from "../services/authService";
import { Link } from "react-router-dom";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Card, CardContent } from "../components/ui/card";
import { Loader2 } from "lucide-react";

export default function ResendVerificationPage() {
  const [email, setEmail] = useState("");
  const [status, setStatus] = useState("idle");
  const [error, setError] = useState("");
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setStatus("loading");
    setError("");
    try {
      await resendVerification(email);
      setStatus("success");
    } catch (err) {
      console.error("Resend error:", err);
      setError(err.message || "Ошибка отправки письма");
      setStatus("error");
    }
  };

  return (
    <div className="flex items-center justify-center">
      <Card className="w-full max-w-md">
        <CardContent className="p-8 space-y-6">
          <div className="flex justify-center">
            <Link to="/">
              <img src="/logo.svg" alt="CookIt" className="h-15" />
            </Link>
          </div>

          {status === "success" ? (
            <div className="text-center space-y-4">
              <h2 className="text-2xl font-bold">Письмо отправлено!</h2>
              <p className="text-muted-foreground">
                На адрес <span className="font-medium">{email}</span> отправлено письмо с подтверждением.
              </p>
              <Button variant="violet" className="w-full" onClick={() => navigate("/")}>
                На главную
              </Button>
            </div>
          ) : (
            <>
              <h2 className="text-2xl font-bold text-center">Повторная отправка</h2>
              <form onSubmit={handleSubmit} className="space-y-4">
                <div className="space-y-2">
                  <label className="text-sm font-medium">Email</label>
                  <Input
                    type="email"
                    placeholder="example@mail.com"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                  />
                </div>
                {error && (
                  <p className="text-sm text-destructive text-center">{error}</p>
                )}
                <Button type="submit" variant="violet" className="w-full" disabled={status === "loading"}>
                  {status === "loading" && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                  Отправить письмо
                </Button>
              </form>
              <div className="text-center">
                <Link to="/login" className="text-sm text-primary hover:underline">
                  Вернуться к входу
                </Link>
              </div>
            </>
          )}
        </CardContent>
      </Card>
    </div>
  );
}