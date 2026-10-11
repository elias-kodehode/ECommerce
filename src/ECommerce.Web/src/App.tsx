import { Route, Routes } from "react-router";
import {
  GuestOnlyRoute,
  RequireAuthRoute,
  RouteFallback,
} from "@/app/route-guards";
import { LoginPage } from "@/features/auth/pages/login-page";
import { RegisterPage } from "@/features/auth/pages/register-page";
import { HomePage } from "@/features/home/home-page";

export default function App() {
  return (
    <Routes>
      <Route element={<GuestOnlyRoute />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
      </Route>

      <Route element={<RequireAuthRoute />}>
        <Route path="/" element={<HomePage />} />
      </Route>

      <Route path="*" element={<RouteFallback />} />
    </Routes>
  );
}
