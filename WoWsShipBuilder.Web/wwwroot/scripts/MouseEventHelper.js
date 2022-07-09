export function PreventMiddleClickDefault(id) {
   const element = document.getElementById(id);
   element.addEventListener("mousedown", function(e){ if(e.button === 1){ e.preventDefault(); return false;} });
}

