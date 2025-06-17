"use client"

import { useState } from "react"
import LandingPage from "../components/LandingPage"
import LoginForm from "../components/LoginForm"
import RegisterForm from "../components/RegisterForm"
import "./globals.css"

export default function App() {
  const [currentView, setCurrentView] = useState("landing") // 'landing', 'login', 'register'

  const showLanding = () => setCurrentView("landing")
  const showLogin = () => setCurrentView("login")
  const showRegister = () => setCurrentView("register")

  return (
    <div className="app">
      {currentView === "landing" && <LandingPage onShowLogin={showLogin} onShowRegister={showRegister} />}
      {currentView === "login" && <LoginForm onBack={showLanding} onShowRegister={showRegister} />}
      {currentView === "register" && <RegisterForm onBack={showLanding} onShowLogin={showLogin} />}
    </div>
  )
}
