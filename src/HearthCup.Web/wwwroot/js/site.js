"use strict";
const design = document.getElementById("designSelect");
if (design) design.addEventListener("change", () => {
 const preview = document.getElementById("designPreview");
 if (/^amazon-[a-z0-9]{10}$/.test(design.value)) preview.src = "/images/products/" + design.value.slice(7) + ".jpg";
 else if (/^[a-z]+-[a-z]+$/.test(design.value)) preview.src = "/images/" + design.value + ".svg";
});
