const animatedSelector = [
  ".page-heading",
  ".page-heading__card",
  ".hero-panel",
  ".hero-card",
  ".stat-card",
  ".feature-card",
  ".panel-card",
  ".preview-card",
  ".sql-card",
  ".feed-item",
  ".field-grid > div",
  ".data-form button",
  ".civic-table tbody tr",
  ".contact-chip",
  ".site-footer .footer-card"
].join(",");

document.addEventListener("DOMContentLoaded", () => {
  markActiveNav();
  prepareAnimations();
  wireFilters();
  wireConfirmationModal();
});

function markActiveNav() {
  const currentPath = window.location.pathname.toLowerCase();

  document.querySelectorAll(".nav-link").forEach((link) => {
    const href = (link.getAttribute("href") || "").toLowerCase();
    if (href && (currentPath === href || (currentPath === "/" && href.endsWith("/portal")))) {
      link.classList.add("is-active");
    }
  });
}

function prepareAnimations() {
  const items = Array.from(document.querySelectorAll(animatedSelector));
  if (!items.length) {
    return;
  }

  items.forEach((item, index) => {
    item.dataset.animate = getAnimationDirection(item, index);
    item.style.setProperty("--delay", `${Math.min(index * 60, 540)}ms`);
  });

  const prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  if (prefersReducedMotion || !("IntersectionObserver" in window)) {
    items.forEach((item) => item.classList.add("in-view"));
    return;
  }

  const observer = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        entry.target.classList.add("in-view");
        observer.unobserve(entry.target);
      }
    });
  }, {
    threshold: 0.1,
    rootMargin: "0px 0px -48px 0px"
  });

  items.forEach((item) => observer.observe(item));
}

function getAnimationDirection(item, index) {
  if (item.matches(".hero-panel, .hero-card, .stat-card, .feature-card")) {
    return "zoom";
  }

  if (item.matches(".page-heading, .panel-card:nth-of-type(odd), .contact-chip:nth-of-type(odd)")) {
    return "left";
  }

  if (item.matches(".page-heading__card, .panel-card:nth-of-type(even), .contact-chip:nth-of-type(even), .civic-table tbody tr:nth-child(even)")) {
    return "right";
  }

  return index % 2 === 0 ? "left" : "right";
}

function wireFilters() {
  document.querySelectorAll("[data-filter-input]").forEach((input) => {
    const targetSelector = input.getAttribute("data-filter-input");
    const container = document.querySelector(targetSelector);

    if (!container) {
      return;
    }

    const items = Array.from(container.querySelectorAll("[data-filter-item]"));
    input.addEventListener("input", () => {
      const query = input.value.trim().toLowerCase();

      items.forEach((item) => {
        const haystack = (item.getAttribute("data-filter-text") || item.textContent || "").toLowerCase();
        const matches = !query || haystack.includes(query);
        item.style.display = matches ? "" : "none";
      });
    });
  });
}

function wireConfirmationModal() {
  const modal = document.querySelector("#confirmation-modal");
  if (!modal) {
    return;
  }

  const title = modal.querySelector("#confirmation-modal-title");
  const message = modal.querySelector("#confirmation-modal-message");
  const badge = modal.querySelector("#confirmation-modal-badge");
  const cancelButton = modal.querySelector("[data-confirm-cancel='true']");
  const continueButton = modal.querySelector("[data-confirm-continue='true']");
  let lastTrigger = null;
  let pendingAction = null;

  const closeModal = () => {
    modal.hidden = true;
    document.body.style.overflow = "";
    pendingAction = null;

    if (lastTrigger instanceof HTMLElement) {
      lastTrigger.focus();
    }
  };

  document.querySelectorAll("[data-confirm-action='true']").forEach((trigger) => {
    trigger.addEventListener("click", (event) => {
      event.preventDefault();

      lastTrigger = trigger;
      const confirmTitle = trigger.getAttribute("data-confirm-title") || "Are you sure?";
      const confirmMessage = trigger.getAttribute("data-confirm-message") || "Please confirm that you want to continue with this action.";
      const confirmButton = trigger.getAttribute("data-confirm-button") || "Continue";
      const confirmBadge = trigger.getAttribute("data-confirm-badge") || "Please Confirm";

      if (title) {
        title.textContent = confirmTitle;
      }

      if (message) {
        message.textContent = confirmMessage;
      }

      if (badge) {
        badge.textContent = confirmBadge;
      }

      if (continueButton) {
        continueButton.textContent = confirmButton;
      }

      if (trigger.matches("a[href]")) {
        const href = trigger.getAttribute("href") || "/";
        pendingAction = () => {
          window.location.href = href;
        };
      } else if (trigger instanceof HTMLButtonElement && trigger.form) {
        pendingAction = () => {
          trigger.form.submit();
        };
      } else {
        pendingAction = null;
      }

      modal.hidden = false;
      document.body.style.overflow = "hidden";
      cancelButton?.focus();
    });
  });

  cancelButton?.addEventListener("click", closeModal);
  continueButton?.addEventListener("click", () => {
    if (typeof pendingAction === "function") {
      pendingAction();
    }
  });

  modal.addEventListener("click", (event) => {
    if (event.target === modal) {
      closeModal();
    }
  });

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape" && !modal.hidden) {
      closeModal();
    }
  });
}
