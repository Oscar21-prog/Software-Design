CREATE DATABASE IF NOT EXISTS euroglobe_bank
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE euroglobe_bank;

CREATE TABLE users (
  id INT AUTO_INCREMENT PRIMARY KEY,
  email VARCHAR(255) NOT NULL UNIQUE,
  password_hash VARCHAR(255) NOT NULL,
  first_name VARCHAR(100) NOT NULL,
  last_name VARCHAR(100) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
)

CREATE TABLE accounts (
  id INT AUTO_INCREMENT PRIMARY KEY,
  user_id INT NOT NULL,
  iban VARCHAR(34) NOT NULL UNIQUE,
  account_type ENUM('CHECKING', 'SAVINGS') NOT NULL,
  balance DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  status ENUM('ACTIVE', 'BLOCKED', 'CLOSED') NOT NULL DEFAULT 'ACTIVE',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (user_id) REFERENCES users(id)
)

CREATE TABLE transactions (
  id INT AUTO_INCREMENT PRIMARY KEY,
  account_id INT NOT NULL,
  type ENUM(
    'DEPOSIT',
    'WITHDRAWAL',
    'CARD_PAYMENT',
    'TRANSFER',
    'ROUND_UP'
  ) NOT NULL,
  amount DECIMAL(12,2) NOT NULL,
  description VARCHAR(255),
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (account_id) REFERENCES accounts(id)
)

CREATE TABLE cards (
  id INT AUTO_INCREMENT PRIMARY KEY,
  user_id INT NOT NULL,
  account_id INT NOT NULL,
  card_number_masked VARCHAR(19) NOT NULL,
  expiry_date DATE NOT NULL,
  status ENUM('ACTIVE', 'BLOCKED', 'CANCELLED') NOT NULL DEFAULT 'ACTIVE',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (user_id) REFERENCES users(id),
  FOREIGN KEY (account_id) REFERENCES accounts(id)
)

CREATE TABLE mortgages (
  id INT AUTO_INCREMENT PRIMARY KEY,
  user_id INT NOT NULL,
  amount DECIMAL(12,2) NOT NULL,
  term_months INT NOT NULL,
  interest_rate DECIMAL(5,2) NOT NULL,
  status ENUM(
    'PENDING',
    'APPROVED',
    'REJECTED',
    'ACTIVE',
    'CLOSED'
  ) NOT NULL DEFAULT 'PENDING',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (user_id) REFERENCES users(id)
)

CREATE TABLE leasing_contracts (
  id INT AUTO_INCREMENT PRIMARY KEY,
  user_id INT NOT NULL,
  vehicle_model VARCHAR(150) NOT NULL,
  monthly_fee DECIMAL(12,2) NOT NULL,
  term_months INT NOT NULL,
  status ENUM(
    'PENDING',
    'APPROVED',
    'REJECTED',
    'ACTIVE',
    'CLOSED'
  ) NOT NULL DEFAULT 'PENDING',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (user_id) REFERENCES users(id)
)

CREATE TABLE vaults (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(100) NOT NULL,
  owner_id INT NOT NULL,
  required_signatures INT NOT NULL DEFAULT 2,
  balance DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  status ENUM('ACTIVE', 'CLOSED') NOT NULL DEFAULT 'ACTIVE',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (owner_id) REFERENCES users(id)
)

CREATE TABLE vault_members (
  id INT AUTO_INCREMENT PRIMARY KEY,
  vault_id INT NOT NULL,
  user_id INT NOT NULL,
  role ENUM('OWNER', 'MEMBER') NOT NULL DEFAULT 'MEMBER',
  joined_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY unique_member (vault_id, user_id),
  FOREIGN KEY (vault_id) REFERENCES vaults(id),
  FOREIGN KEY (user_id) REFERENCES users(id)
)

CREATE TABLE vault_approvals (
  id INT AUTO_INCREMENT PRIMARY KEY,
  vault_id INT NOT NULL,
  user_id INT NOT NULL,
  approved_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY unique_approval (vault_id, user_id),
  FOREIGN KEY (vault_id) REFERENCES vaults(id),
  FOREIGN KEY (user_id) REFERENCES users(id)
)