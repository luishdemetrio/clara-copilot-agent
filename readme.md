# 👧 CLARA -Copilot License Assignment & Report Agent

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Platform](https://img.shields.io/badge/Platform-Microsoft%20Copilot%20Studio-blue)](https://copilotstudio.microsoft.com/)
[![.NET](https://img.shields.io/badge/.NET-REST%20API-purple)](https://dotnet.microsoft.com/)

**Clara** is an intelligent AI agent built on Microsoft Copilot Studio that revolutionizes M365 Copilot license management for enterprises. It automates license monitoring, optimizes allocation, and streamlines user communication to ensure maximum ROI on your M365 Copilot investment.

![](images/Clara.png)

---

## 🚀 What is Clara?

Clara helps IT teams:
- Monitor and analyze Copilot license usage
- Manage waitlists and automate license assignments
- Communicate with users about license status

---
## 🎯 Overview

Managing M365 Copilot licenses across large organizations can be complex and time-consuming. Clara solves this by providing:

- **Automated License Monitoring**: Real-time tracking of license usage and identification of inactive users
- **Intelligent Waitlist Management**: Streamlined process for managing license requests and approvals
- **Smart Reassignment Workflows**: Automated redistribution of unused licenses to waiting users
- **Proactive Communication**: Automated notifications to users about license status and usage optimization

![](images/clara_overview.png)

---
## 📚 Documentation & Support

All setup guides, usage instructions, and troubleshooting are now in our Wiki and Discussions.

- **Getting Started:** See the [Wiki](https://github.com/luishdemetrio/clara-copilot-agent/wiki)
- **Ask Questions:** Use [Discussions](https://github.com/luishdemetrio/clara-copilot-agent/discussions)
- **Report Issues:** GitHub [Issues](https://github.com/luishdemetrio/clara-copilot-agent/issues)

---


## 🏗 Architecture

```mermaid
graph TB
    A[Clara Agent] --> B[.NET REST APIs]
    A --> C[SharePoint Lists]
    A --> D[Power Automate]
    
    B --> E[M365 Copilot Usage Dashboard]
    C --> F[Waitlist Management]
    D --> G[Email Notifications]
    D --> L[Teams Notifications]
    
    E --> H[Usage Analytics]
    F --> I[User Tracking]
    G --> J[Automated Communications]
    L --> J[Automated Communications]
```


## 🤝 Contributing

We welcome contributions!

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](https://opensource.org/license/MIT) file for details.

## 🌟 Acknowledgments

- Microsoft Copilot Studio team for the amazing platform
- Microsoft Graph API for comprehensive M365 integration
- The enterprise IT community for valuable feedback and requirements

---

**⭐ If Clara helps optimize your M365 Copilot license management, please star this repository!**

Made with ❤️ for enterprise IT teams worldwide.




