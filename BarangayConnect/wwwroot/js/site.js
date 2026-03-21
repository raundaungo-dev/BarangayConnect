const animatedSelector = [
  ".page-heading",
  ".hero-panel",
  ".hero-card",
  ".stat-card",
  ".panel-card",
  ".feed-item",
  ".field-grid > div",
  ".data-form button",
  ".civic-table tbody tr",
  ".site-footer .container"
].join(",");

document.addEventListener("DOMContentLoaded", () => {
  markActiveNav();
  prepareAnimations();
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
    item.style.setProperty("--delay", `${Math.min(index * 70, 480)}ms`);
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
    threshold: 0.12,
    rootMargin: "0px 0px -40px 0px"
  });

  items.forEach((item) => observer.observe(item));
}

function getAnimationDirection(item, index) {
  if (item.matches(".hero-panel, .hero-card, .stat-card")) {
    return "zoom";
  }

  if (item.matches(".page-heading")) {
    return "left";
  }

  if (item.matches(".field-grid > div:nth-child(even), .civic-table tbody tr:nth-child(even)")) {
    return "right";
  }

  return index % 2 === 0 ? "left" : "right";
}
