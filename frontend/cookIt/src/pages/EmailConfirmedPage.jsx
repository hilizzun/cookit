import React from "react";
import { Link } from "react-router-dom";
import { Button } from "../components/ui/button";
import { Card, CardContent } from "../components/ui/card";

export default function EmailConfirmedPage() {
  return (
    <div className="flex items-center justify-center min-h-screen p-4">
      <Card className="w-full max-w-md">
        <CardContent className="p-8 space-y-6">
          <div className="flex justify-center">
            <Link to="/">
              <img src="/logo.svg" alt="CookIt" className="h-15" />
            </Link>
          </div>
          <div className="text-center space-y-4">
            <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto">
              <svg className="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              </svg>
            </div>
            <h2 className="text-2xl font-bold">Email подтверждён!</h2>
            <p className="text-muted-foreground">Ваш аккаунт активирован. Теперь вы можете войти.</p>
            <Button variant="violet" className="w-full" asChild>
              <Link to="/login">Войти</Link>
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}