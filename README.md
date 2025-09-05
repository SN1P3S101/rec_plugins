# [REC] Rust Plugins
![Status](https://img.shields.io/badge/status-Active-orange)
[![License](https://img.shields.io/badge/license-Proprietary-purple)](./LICENSE)
![Availability](https://img.shields.io/badge/Plugins-Free%20%26%20Paid-blue)
![GitHub issues](https://img.shields.io/github/issues/SN1P3S101/rec_plugins)
![GitHub closed issues](https://img.shields.io/github/issues-closed/SN1P3S101/rec_plugins)

I am developing these plugins as part of my journey to learn, experiment, and actively use them on my own Rust servers.  
The goal is to create high-quality, modular plugins that can serve as a strong foundation. Other developers might also learn from these plugins or extend them by using the provided hooks and APIs.

- ✅ Free plugins will be available directly from this repository.  
- 💲 Advanced/paid plugins will also be listed here, but require a valid purchase on the [webstore](https://recaris.eu) before they can be used in any live or commercial environment (including servers where revenue is generated).  

---

## ⚖️ License
The code in this repository is protected under a **Proprietary License**.  
You may study the code, use it for learning, and build your own plugins that rely on the public hooks.  
Live or commercial use requires a valid purchase.  
For full details, see [LICENSE](./LICENSE).

---

## 📦 Purpose
This project serves two goals:  
1. **Learning & Experimentation** — It is part of my own development journey to design modular Rust plugins and document the process.  
2. **Foundation & Control** — It provides a complete ecosystem of plugins for running high-quality servers under the Recaris Gaming brand, while keeping licensing under control.  

With this repository I aim to:  
- Improve my own skills while documenting the entire process.  
- Provide a transparent overview of plugin structure and dependencies.  
- Share a set of hooks and APIs that others can build on.  
- Allow free educational use, but keep commercial rights protected via the [webstore](https://recaris.eu).  

> 💡 Note: Although the default setup uses the **Recaris Gaming** brand (name, colors, tags), these can easily be customized to fit **your own server’s branding**.

---

## 📋 Plugins

- [ ] **rec_config** (Planned)  
  Centralized configuration provider (schema-driven, live reload).  

- [ ] **rec_database** (Planned)  
  Database connection & schema migrations.  

- [ ] **rec_core** (Planned)  
  Core functionality: welcome messages, commands, utilities.  

- [ ] **rec_ui** (Planned)  
  UI component library (Carbon LUI wrappers).  

- [ ] **rec_messages** (Planned)  
  Chat pipeline, ranks/tags integration, hide unwanted notices, periodic info messages.  

- [ ] **rec_ranks** (Planned)  
  Rank management & progression.  

- [ ] **rec_tags** (Planned)  
  Custom name tags/prefixes.  

- [ ] **rec_permissions** (Planned)  
  Permission layer on top of Carbon.  

- [ ] **rec_events** (Planned)  
  Event dispatcher (join/leave/raid/etc.) with in-chat notifications.  

- [ ] **rec_admin** (Planned)  
  Admin menu with UI and permission-based access.  

- [ ] **rec_protection** (Planned)  
  Offline raid protection system.  

- [ ] **rec_checker** (Planned)  
  Health checks, plugin update checks, external webhook bridge.  

- [ ] **rec_gather** (Planned)  
  Resource gather management.  

- [ ] **rec_stacker** (Planned)  
  Stack size tuning.  

- [ ] **rec_shop** (Planned)  
  Shop system (items/kits).  

- [ ] **rec_economy** (Planned)  
  Currency & transactions system.  

- [ ] **rec_status** (Planned)  
  Status messages & announcements.  

- [ ] **rec_discord** (Planned)  
  Discord integration (account linking, relays).  

- [ ] **rec_scheduler** (Optional, Planned)  
  Centralized cron/timer system for all plugins.  

- [ ] **rec_cache** (Optional, Planned)  
  In-memory cache layer for fast lookups.  

- [ ] **rec_api** (Optional, Planned)  
  External API endpoints (for trusted integrations).  

- [ ] **rec_localization** (Optional, Planned)  
  Central language/translation manager.  

---

## 👨‍💻 Developer Guide
Public hooks and APIs will be documented per plugin as they are implemented.  

---

## 🤝 Contributions
Issues for feedback and feature requests are welcome.   

---

© 2025 Recaris Gaming. All rights reserved.
