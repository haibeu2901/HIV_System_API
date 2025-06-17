"use client"

export default function Header({ onShowLogin, onShowRegister }) {
  return (
    <header className="header">
      <div className="container">
        <div className="nav">
          <div className="logo">
            <h2>HIV Care+</h2>
          </div>
          <div className="nav-links">
            <button className="nav-link">About</button>
            <button className="nav-link">Services</button>
            <button className="nav-link">Contact</button>
            <button className="btn-secondary" onClick={onShowLogin}>
              Login
            </button>
            <button className="btn-primary" onClick={onShowRegister}>
              Register
            </button>
          </div>
        </div>
      </div>
    </header>
  )
}
