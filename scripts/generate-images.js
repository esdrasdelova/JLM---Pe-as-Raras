/*
 * JLM — gerador de arte vetorial de produto.
 * Executar: node scripts/generate-images.js
 * Gera imagens SVG coerentes com a identidade preta/dourada da marca.
 */
const fs = require("fs");
const path = require("path");

const OUT = path.join(__dirname, "..", "JLM", "wwwroot", "images", "products");
fs.mkdirSync(OUT, { recursive: true });

const palettes = {
  black: { body: "#1c1c1c", body2: "#111111", line: "#4a463f", hi: "#2b2a27", ink: "#C9A45C" },
  graphite: { body: "#2c2c2c", body2: "#181818", line: "#56514a", hi: "#3a3936", ink: "#C9A45C" },
  cream: { body: "#E9E2D5", body2: "#D5CCBB", line: "#A79D8B", hi: "#F4EFE6", ink: "#8a6f3d" },
  gold: { body: "#C9A45C", body2: "#A98543", line: "#E6CE9A", hi: "#E3C27E", ink: "#17130b" },
  brown: { body: "#4A3A2E", body2: "#33271F", line: "#7B6350", hi: "#5A4838", ink: "#C9A45C" },
};

function frame(inner, { label = "", accent = "#C9A45C" } = {}) {
  return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 900 1125" role="img" aria-label="${label}">
  <defs>
    <linearGradient id="bg" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0" stop-color="#171614"/><stop offset=".55" stop-color="#111110"/><stop offset="1" stop-color="#080808"/>
    </linearGradient>
    <radialGradient id="glow" cx="50%" cy="36%" r="62%">
      <stop offset="0" stop-color="${accent}" stop-opacity=".18"/><stop offset=".6" stop-color="${accent}" stop-opacity=".04"/><stop offset="1" stop-color="${accent}" stop-opacity="0"/>
    </radialGradient>
    <linearGradient id="fabric" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0" class="fabric-hi"/><stop offset=".5" class="fabric-body"/><stop offset="1" class="fabric-body2"/>
    </linearGradient>
    <filter id="grain"><feTurbulence type="fractalNoise" baseFrequency=".85" numOctaves="3" stitchTiles="stitch"/><feColorMatrix type="saturate" values="0"/><feComponentTransfer><feFuncA type="linear" slope=".06"/></feComponentTransfer></filter>
    <filter id="soft"><feGaussianBlur stdDeviation="18"/></filter>
  </defs>
  <rect width="900" height="1125" fill="url(#bg)"/>
  <rect width="900" height="1125" fill="url(#glow)"/>
  <g fill="none" stroke="${accent}" opacity=".10">
    <circle cx="450" cy="470" r="330"/><circle cx="450" cy="470" r="250"/><circle cx="450" cy="470" r="170"/>
    <path d="M80 950 L820 190"/><path d="M60 760 L720 60"/><path d="M180 1080 L900 340"/>
  </g>
  <g opacity=".5" fill="${accent}">
    <circle cx="118" cy="132" r="2.5"/><circle cx="782" cy="132" r="2.5"/><circle cx="118" cy="992" r="2.5"/><circle cx="782" cy="992" r="2.5"/>
  </g>
  <ellipse cx="450" cy="885" rx="250" ry="42" fill="#000" opacity=".55" filter="url(#soft)"/>
  ${inner}
  <g font-family="Georgia, 'Times New Roman', serif" text-anchor="middle">
    <text x="450" y="1032" fill="${accent}" font-size="27" letter-spacing="11">JLM</text>
    <text x="450" y="1062" fill="#8b8578" font-size="11" letter-spacing="5" font-family="Arial, sans-serif">EST. 2024 · SÃO PAULO</text>
  </g>
  <rect x="28" y="28" width="844" height="1069" fill="none" stroke="${accent}" stroke-opacity=".22"/>
  <rect width="900" height="1125" filter="url(#grain)" opacity=".55"/>
</svg>`;
}

function styleVars(p) {
  return `<style>.fabric-body{stop-color:${p.body}} .fabric-body2{stop-color:${p.body2}} .fabric-hi{stop-color:${p.hi}} .st{stroke:${p.line}} .ink{fill:${p.ink}} .inkst{stroke:${p.ink}}</style>`;
}

const shapes = {
  tee: (p) => `${styleVars(p)}<g stroke-width="3" stroke-linejoin="round">
    <path d="M300 268 Q450 316 600 268 L704 374 L626 452 L582 408 L584 836 Q450 882 316 836 L318 408 L274 452 L196 374 Z" fill="url(#fabric)" stroke="${p.line}"/>
    <path d="M366 246 Q450 330 534 246 Q522 220 496 210 Q450 268 404 210 Q378 220 366 246 Z" fill="${p.body2}" stroke="${p.line}"/>
    <path d="M318 408 L316 836 M584 408 L582 836" stroke="${p.line}" opacity=".5"/>
    <path d="M394 520 L506 520 M404 550 L496 550" stroke="${p.ink}" opacity=".9"/>
    <text x="450" y="640" text-anchor="middle" class="ink" font-family="Georgia, serif" font-size="44" letter-spacing="8">JLM</text>
    <text x="450" y="674" text-anchor="middle" fill="${p.ink}" font-family="Arial, sans-serif" font-size="10" letter-spacing="5" opacity=".75">SIGNATURE</text>
    <path d="M350 760 Q450 786 550 760" fill="none" stroke="${p.line}" opacity=".35"/>
  </g>`,
  hoodie: (p) => `${styleVars(p)}<g stroke-width="3" stroke-linejoin="round">
    <path d="M292 292 Q450 338 608 292 L714 402 L632 482 L588 438 L590 864 Q450 910 310 864 L312 438 L268 482 L186 402 Z" fill="url(#fabric)" stroke="${p.line}"/>
    <path d="M346 286 Q380 170 450 170 Q520 170 554 286 Q500 324 450 324 Q400 324 346 286 Z" fill="${p.body2}" stroke="${p.line}"/>
    <path d="M376 296 Q420 250 450 250 Q480 250 524 296" fill="none" stroke="${p.line}"/>
    <path d="M394 320 L380 452 M506 320 L520 452" stroke="${p.ink}" stroke-width="4"/>
    <circle cx="380" cy="460" r="6" fill="${p.ink}"/><circle cx="520" cy="460" r="6" fill="${p.ink}"/>
    <path d="M334 690 L566 690 L566 812 Q450 844 334 812 Z" fill="${p.body2}" stroke="${p.line}" opacity=".9"/>
    <text x="450" y="590" text-anchor="middle" class="ink" font-family="Georgia, serif" font-size="38" letter-spacing="7">JLM</text>
    <path d="M318 438 L310 864 M582 438 L590 864" stroke="${p.line}" opacity=".4"/>
  </g>`,
  jacket: (p) => `${styleVars(p)}<g stroke-width="3" stroke-linejoin="round">
    <path d="M282 292 L382 250 Q450 292 518 250 L618 292 L722 420 L638 492 L600 444 L602 852 Q450 900 298 852 L300 444 L262 492 L178 420 Z" fill="url(#fabric)" stroke="${p.line}"/>
    <path d="M382 250 L450 360 L518 250 L500 226 L450 300 L400 226 Z" fill="${p.body2}" stroke="${p.line}"/>
    <path d="M450 360 L450 878" stroke="${p.ink}" stroke-width="4"/>
    <path d="M450 378 L450 862" stroke="${p.line}" stroke-dasharray="3 9"/>
    <path d="M330 560 L408 560 L408 640 L330 640 Z M492 560 L570 560 L570 640 L492 640 Z" fill="none" stroke="${p.line}"/>
    <path d="M300 444 L332 700 M600 444 L568 700" stroke="${p.line}" opacity=".4"/>
    <text x="530" y="760" text-anchor="middle" class="ink" font-family="Georgia, serif" font-size="26" letter-spacing="5">JLM</text>
  </g>`,
  shirt: (p) => `${styleVars(p)}<g stroke-width="3" stroke-linejoin="round">
    <path d="M298 282 L398 246 L450 334 L502 246 L602 282 L700 410 L618 480 L584 436 L586 846 Q450 890 314 846 L316 436 L282 480 L200 410 Z" fill="url(#fabric)" stroke="${p.line}"/>
    <path d="M398 246 L450 334 L502 246 L470 220 L450 270 L430 220 Z" fill="${p.body2}" stroke="${p.line}"/>
    <path d="M450 334 L450 856" stroke="${p.line}"/>
    <g fill="${p.ink}"><circle cx="450" cy="420" r="4"/><circle cx="450" cy="492" r="4"/><circle cx="450" cy="564" r="4"/><circle cx="450" cy="636" r="4"/><circle cx="450" cy="708" r="4"/></g>
    <path d="M348 500 L384 500 L384 580 L348 580 Z" fill="none" stroke="${p.line}" opacity=".8"/>
    <text x="342" y="760" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="22" letter-spacing="4">JLM</text>
  </g>`,
  pants: (p) => `${styleVars(p)}<g stroke-width="3" stroke-linejoin="round">
    <path d="M318 268 L582 268 L606 430 L578 862 L468 862 L452 516 L436 862 L326 862 L298 430 Z" fill="url(#fabric)" stroke="${p.line}"/>
    <path d="M318 268 L582 268 L590 356 L310 356 Z" fill="${p.body2}" stroke="${p.line}"/>
    <path d="M310 356 L590 356" stroke="${p.line}"/>
    <path d="M328 300 L316 348 M572 300 L584 348" stroke="${p.ink}" stroke-width="3"/>
    <path d="M352 400 L344 470 M552 400 L560 470" stroke="${p.line}" opacity=".7"/>
    <path d="M400 560 L428 560 M520 560 L548 560" stroke="${p.line}" opacity=".5"/>
    <rect x="390" y="398" width="120" height="74" fill="none" stroke="${p.line}" opacity=".65"/>
    <text x="450" y="446" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="20" letter-spacing="4">JLM</text>
    <path d="M326 862 L436 862 L452 560 M578 862 L468 862" stroke="${p.line}" opacity=".35"/>
  </g>`,
  blazer: (p) => `${styleVars(p)}<g stroke-width="3" stroke-linejoin="round">
    <path d="M272 286 L380 248 Q450 286 520 248 L628 286 L738 438 L646 510 L604 454 L606 862 Q450 910 294 862 L296 454 L254 510 L162 438 Z" fill="url(#fabric)" stroke="${p.line}"/>
    <path d="M380 248 L450 400 L340 470 L320 340 Z" fill="${p.body2}" stroke="${p.line}"/>
    <path d="M520 248 L450 400 L560 470 L580 340 Z" fill="${p.body2}" stroke="${p.line}"/>
    <path d="M450 400 L450 866" stroke="${p.line}"/>
    <path d="M334 574 L420 574 M480 574 L566 574" stroke="${p.line}" opacity=".8"/>
    <circle cx="450" cy="486" r="5" fill="${p.ink}"/><circle cx="450" cy="548" r="5" fill="${p.ink}"/>
    <path d="M582 690 L618 690 L618 740 L582 740 Z" fill="none" stroke="${p.ink}" opacity=".8"/>
  </g>`,
  chain: (p) => `${styleVars(p)}<g fill="none">
    <path d="M312 306 Q312 640 450 700 Q588 640 588 306" stroke="#6b542d" stroke-width="26" opacity=".6"/>
    <path d="M312 306 Q312 640 450 700 Q588 640 588 306" stroke="${p.body}" stroke-width="18"/>
    <path d="M312 306 Q312 640 450 700 Q588 640 588 306" stroke="${p.hi}" stroke-width="9" stroke-dasharray="8 7"/>
    <path d="M312 306 Q312 640 450 700 Q588 640 588 306" stroke="${p.line}" stroke-width="2" stroke-dasharray="8 7"/>
    <circle cx="450" cy="722" r="62" fill="${p.body2}" stroke="${p.hi}" stroke-width="7"/>
    <circle cx="450" cy="722" r="52" fill="none" stroke="${p.line}" stroke-width="1" opacity=".6"/>
    <text x="450" y="712" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="40" letter-spacing="3">J</text>
    <text x="450" y="748" text-anchor="middle" fill="${p.ink}" font-family="Arial, sans-serif" font-size="9" letter-spacing="5">L M</text>
    <path d="M272 290 L312 306 M628 290 L588 306" stroke="${p.body}" stroke-width="18"/>
  </g>`,
  pendant: (p) => `${styleVars(p)}<g fill="none">
    <path d="M318 274 Q318 560 450 620 Q582 560 582 274" stroke="${p.body}" stroke-width="14"/>
    <path d="M318 274 Q318 560 450 620 Q582 560 582 274" stroke="${p.hi}" stroke-width="7" stroke-dasharray="7 6"/>
    <rect x="360" y="628" width="180" height="230" rx="14" fill="${p.body2}" stroke="${p.hi}" stroke-width="6"/>
    <rect x="378" y="646" width="144" height="194" rx="8" fill="none" stroke="${p.line}" opacity=".6"/>
    <text x="450" y="740" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="58" letter-spacing="2">JLM</text>
    <path d="M402 774 L498 774" stroke="${p.ink}" stroke-width="2" opacity=".7"/>
    <text x="450" y="806" text-anchor="middle" fill="${p.ink}" font-family="Arial, sans-serif" font-size="10" letter-spacing="4">PRESENÇA</text>
    <circle cx="450" cy="616" r="13" fill="none" stroke="${p.hi}" stroke-width="5"/>
  </g>`,
  bracelet: (p) => `${styleVars(p)}<g fill="none">
    <ellipse cx="450" cy="560" rx="228" ry="150" stroke="${p.body}" stroke-width="30"/>
    <ellipse cx="450" cy="560" rx="228" ry="150" stroke="${p.hi}" stroke-width="17" stroke-dasharray="16 8"/>
    <ellipse cx="450" cy="560" rx="228" ry="150" stroke="${p.line}" stroke-width="2" stroke-dasharray="16 8"/>
    <ellipse cx="450" cy="560" rx="200" ry="126" stroke="${p.body2}" stroke-width="6" opacity=".7"/>
    <g transform="translate(450 710)">
      <rect x="-62" y="-22" width="124" height="52" rx="10" fill="${p.body2}" stroke="${p.hi}" stroke-width="5"/>
      <text y="14" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="30" letter-spacing="1">JLM</text>
    </g>
    <path d="M222 560 Q222 690 450 710 Q678 690 678 560" stroke="${p.line}" stroke-width="1" opacity=".4"/>
  </g>`,
  ring: (p) => `${styleVars(p)}<g>
    <ellipse cx="450" cy="570" rx="152" ry="176" fill="none" stroke="${p.body2}" stroke-width="44"/>
    <ellipse cx="450" cy="570" rx="152" ry="176" fill="none" stroke="${p.body}" stroke-width="34"/>
    <ellipse cx="450" cy="570" rx="152" ry="176" fill="none" stroke="${p.hi}" stroke-width="6" opacity=".8"/>
    <path d="M450 394 L512 448 L488 512 L412 512 L388 448 Z" fill="${p.body2}" stroke="${p.hi}" stroke-width="5"/>
    <path d="M450 414 L488 452 L470 492 L430 492 L412 452 Z" fill="${p.hi}" opacity=".55"/>
    <path d="M450 394 L450 512 M412 448 L488 448" stroke="${p.line}" opacity=".5"/>
    <text x="450" y="820" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="22" letter-spacing="6">JLM</text>
    <path d="M360 840 L540 840" stroke="${p.ink}" opacity=".4"/>
  </g>`,
  earring: (p) => `${styleVars(p)}<g fill="none">
    <circle cx="450" cy="520" r="168" stroke="${p.body2}" stroke-width="40"/>
    <circle cx="450" cy="520" r="168" stroke="${p.body}" stroke-width="28"/>
    <circle cx="450" cy="520" r="168" stroke="${p.hi}" stroke-width="9" stroke-dasharray="18 9"/>
    <circle cx="450" cy="352" r="18" fill="${p.hi}" stroke="${p.line}" stroke-width="3"/>
    <circle cx="450" cy="352" r="38" fill="none" stroke="${p.body}" stroke-width="8"/>
    <text x="450" y="760" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="22" letter-spacing="6">JLM</text>
  </g>`,
  sunglasses: (p) => `${styleVars(p)}<g>
    <path d="M160 470 Q170 360 290 360 Q408 360 424 470 Q430 556 330 572 Q196 586 168 500 Z" fill="${p.body2}" stroke="${p.hi}" stroke-width="8"/>
    <path d="M476 470 Q492 360 610 360 Q730 360 740 470 Q732 586 598 572 Q498 556 476 470 Z" fill="${p.body2}" stroke="${p.hi}" stroke-width="8"/>
    <path d="M424 470 Q450 440 476 470" fill="none" stroke="${p.hi}" stroke-width="9"/>
    <path d="M168 500 L74 462 M732 500 L826 462" stroke="${p.hi}" stroke-width="14" stroke-linecap="round"/>
    <path d="M202 430 Q260 390 340 420" fill="none" stroke="#fff" stroke-opacity=".18" stroke-width="10"/>
    <path d="M518 430 Q576 390 656 420" fill="none" stroke="#fff" stroke-opacity=".18" stroke-width="10"/>
    <text x="300" y="500" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="22" opacity=".7">JLM</text>
    <text x="600" y="500" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="22" opacity=".7">JLM</text>
  </g>`,
  watch: (p) => `${styleVars(p)}<g>
    <rect x="382" y="204" width="136" height="190" rx="34" fill="${p.body2}" stroke="${p.line}" stroke-width="5"/>
    <rect x="382" y="740" width="136" height="190" rx="34" fill="${p.body2}" stroke="${p.line}" stroke-width="5"/>
    <circle cx="450" cy="568" r="196" fill="${p.body2}" stroke="${p.hi}" stroke-width="10"/>
    <circle cx="450" cy="568" r="174" fill="${p.body}" stroke="${p.line}" stroke-width="3"/>
    <circle cx="450" cy="568" r="148" fill="none" stroke="${p.line}" opacity=".4"/>
    <g stroke="${p.hi}" stroke-width="5"><path d="M450 406 L450 428"/><path d="M450 708 L450 730"/><path d="M288 568 L310 568"/><path d="M590 568 L612 568"/></g>
    <g stroke="${p.hi}" stroke-width="3" opacity=".6">${Array.from({ length: 12 }, (_, i) => { const a = (i * 30) * Math.PI / 180; const x1 = 450 + Math.cos(a) * 132, y1 = 568 + Math.sin(a) * 132, x2 = 450 + Math.cos(a) * 142, y2 = 568 + Math.sin(a) * 142; return `<path d="M${x1.toFixed(1)} ${y1.toFixed(1)} L${x2.toFixed(1)} ${y2.toFixed(1)}"/>`; }).join("")}</g>
    <path d="M450 568 L450 466" stroke="${p.ink}" stroke-width="7" stroke-linecap="round"/>
    <path d="M450 568 L532 610" stroke="${p.ink}" stroke-width="5" stroke-linecap="round"/>
    <circle cx="450" cy="568" r="10" fill="${p.ink}"/>
    <text x="450" y="650" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="21" letter-spacing="4">JLM</text>
  </g>`,
  cap: (p) => `${styleVars(p)}<g stroke-linejoin="round">
    <path d="M250 650 Q250 350 450 350 Q650 350 650 650 Z" fill="url(#fabric)" stroke="${p.line}" stroke-width="4"/>
    <path d="M450 350 L450 650" stroke="${p.line}" opacity=".45"/>
    <path d="M250 650 Q450 706 650 650 L770 710 Q740 770 620 780 L250 720 Z" fill="${p.body2}" stroke="${p.line}" stroke-width="4"/>
    <path d="M250 650 Q450 706 650 650" fill="none" stroke="${p.ink}" stroke-width="5"/>
    <circle cx="450" cy="350" r="14" fill="${p.body2}" stroke="${p.line}" stroke-width="4"/>
    <text x="450" y="540" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="64" letter-spacing="7">JLM</text>
    <text x="450" y="582" text-anchor="middle" fill="${p.ink}" font-family="Arial, sans-serif" font-size="11" letter-spacing="5" opacity=".7">SIGNATURE</text>
  </g>`,
  backpack: (p) => `${styleVars(p)}<g stroke-linejoin="round">
    <path d="M300 330 Q300 230 450 230 Q600 230 600 330 L640 810 Q450 866 260 810 Z" fill="url(#fabric)" stroke="${p.line}" stroke-width="4"/>
    <path d="M330 260 Q450 160 570 260" fill="none" stroke="${p.body2}" stroke-width="34"/>
    <path d="M330 260 Q450 160 570 260" fill="none" stroke="${p.line}" stroke-width="3"/>
    <path d="M300 500 L600 500 M300 500 L300 810 M600 500 L600 810" stroke="${p.line}" opacity=".5"/>
    <path d="M356 640 L544 640 L544 760 Q450 790 356 760 Z" fill="${p.body2}" stroke="${p.line}" stroke-width="3"/>
    <rect x="430" y="668" width="40" height="16" rx="8" fill="${p.ink}"/>
    <path d="M316 380 L330 400 M584 380 L570 400" stroke="${p.ink}" stroke-width="4"/>
    <text x="450" y="590" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="28" letter-spacing="5">JLM</text>
  </g>`,
  belt: (p) => `${styleVars(p)}<g>
    <path d="M180 500 Q450 460 720 500 L712 620 Q450 582 188 620 Z" fill="url(#fabric)" stroke="${p.line}" stroke-width="4"/>
    <path d="M180 560 Q450 520 720 560" stroke="${p.line}" opacity=".5"/>
    <g fill="none" stroke="${p.line}" opacity=".7"><circle cx="300" cy="545" r="6"/><circle cx="420" cy="539" r="6"/><circle cx="540" cy="545" r="6"/></g>
    <rect x="660" y="468" width="130" height="180" rx="16" fill="${p.body2}" stroke="${p.hi}" stroke-width="7"/>
    <rect x="684" y="498" width="82" height="120" rx="8" fill="none" stroke="${p.line}" stroke-width="3"/>
    <path d="M725 468 L725 648" stroke="${p.ink}" stroke-width="5" opacity=".8"/>
    <text x="450" y="780" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="24" letter-spacing="7">JLM</text>
  </g>`,
  wallet: (p) => `${styleVars(p)}<g>
    <rect x="230" y="340" width="440" height="500" rx="26" fill="url(#fabric)" stroke="${p.line}" stroke-width="4"/>
    <rect x="258" y="368" width="384" height="444" rx="16" fill="none" stroke="${p.line}" opacity=".5"/>
    <path d="M230 540 L670 540" stroke="${p.line}" opacity=".6"/>
    <path d="M230 780 Q450 742 670 780" fill="${p.body2}" opacity=".8"/>
    <rect x="376" y="586" width="148" height="92" rx="8" fill="${p.body2}" stroke="${p.hi}" stroke-width="3"/>
    <text x="450" y="644" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="34" letter-spacing="2">JLM</text>
    <circle cx="600" cy="470" r="10" fill="${p.ink}"/>
  </g>`,
  sneaker: (p) => `${styleVars(p)}<g stroke-linejoin="round">
    <path d="M180 680 Q200 560 300 560 L372 620 L468 640 L604 700 Q700 720 726 790 L726 830 Q640 856 474 848 L212 830 Q172 810 180 760 Z" fill="url(#fabric)" stroke="${p.line}" stroke-width="4"/>
    <path d="M198 826 Q450 866 724 830 L724 872 Q620 902 458 894 L220 878 Q192 866 198 826 Z" fill="${p.body2}" stroke="${p.line}" stroke-width="4"/>
    <path d="M380 624 Q430 700 444 838 M446 644 Q500 720 516 840 M512 666 Q562 746 574 842" fill="none" stroke="${p.line}" stroke-width="3" opacity=".8"/>
    <path d="M604 700 Q664 716 698 764" stroke="${p.ink}" stroke-width="7" stroke-linecap="round"/>
    <text x="310" y="748" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="30" letter-spacing="4">JLM</text>
    <path d="M250 790 Q330 812 420 800" stroke="${p.line}" opacity=".4"/>
  </g>`,
  bag: (p) => `${styleVars(p)}<g stroke-linejoin="round">
    <path d="M260 440 L640 400 L684 790 Q450 840 224 790 Z" fill="url(#fabric)" stroke="${p.line}" stroke-width="4"/>
    <path d="M300 430 Q300 300 450 290 Q600 300 600 390" fill="none" stroke="${p.body2}" stroke-width="26"/>
    <path d="M300 430 Q300 300 450 290 Q600 300 600 390" fill="none" stroke="${p.line}" stroke-width="3"/>
    <path d="M224 560 Q450 600 684 560" stroke="${p.line}" opacity=".5"/>
    <rect x="400" y="616" width="100" height="58" rx="6" fill="${p.body2}" stroke="${p.hi}" stroke-width="3"/>
    <text x="450" y="654" text-anchor="middle" fill="${p.ink}" font-family="Georgia, serif" font-size="24" letter-spacing="2">JLM</text>
    <path d="M260 440 L224 790 M640 400 L684 790" stroke="${p.line}" opacity=".35"/>
  </g>`,
};

const products = [
  { slug: "camiseta-oversized-signature", type: "tee", palette: "black", accent: "#C9A45C", label: "Camiseta Oversized Signature" },
  { slug: "camiseta-boxy-heavy-cotton", type: "tee", palette: "cream", accent: "#A98B4F", label: "Camiseta Boxy Heavy Cotton" },
  { slug: "moletom-oversized-presenca", type: "hoodie", palette: "black", accent: "#C9A45C", label: "Moletom Oversized Presença" },
  { slug: "jaqueta-bomber-urban-gold", type: "jacket", palette: "graphite", accent: "#C9A45C", label: "Jaqueta Bomber Urban Gold" },
  { slug: "camisa-linho-noir", type: "shirt", palette: "black", accent: "#C9A45C", label: "Camisa Linho Noir" },
  { slug: "calca-cargo-utility", type: "pants", palette: "graphite", accent: "#C9A45C", label: "Calça Cargo Utility" },
  { slug: "calca-alfaiataria-slim", type: "pants", palette: "black", accent: "#C9A45C", label: "Calça Alfaiataria Slim" },
  { slug: "camiseta-longline-essential", type: "tee", palette: "cream", accent: "#A98B4F", label: "Camiseta Longline Essential" },
  { slug: "blazer-premium-gold-line", type: "blazer", palette: "black", accent: "#C9A45C", label: "Blazer Premium Gold Line" },
  { slug: "conjunto-streetwear-jlm", type: "hoodie", palette: "graphite", accent: "#C9A45C", label: "Conjunto Streetwear JLM" },
  { slug: "corrente-cubana-ouro-18k", type: "chain", palette: "gold", accent: "#E3C27E", label: "Corrente Cubana Ouro 18k" },
  { slug: "pingente-jlm-monograma", type: "pendant", palette: "gold", accent: "#E3C27E", label: "Pingente JLM Monograma" },
  { slug: "pulseira-elos-cubanos", type: "bracelet", palette: "gold", accent: "#E3C27E", label: "Pulseira Elos Cubanos" },
  { slug: "anel-signature-onyx", type: "ring", palette: "black", accent: "#C9A45C", label: "Anel Signature Onyx" },
  { slug: "anel-duplo-bold", type: "ring", palette: "gold", accent: "#E3C27E", label: "Anel Duplo Bold" },
  { slug: "brinco-argola-bold", type: "earring", palette: "gold", accent: "#E3C27E", label: "Brinco Argola Bold" },
  { slug: "colar-cordao-vibe", type: "chain", palette: "gold", accent: "#E3C27E", label: "Colar Cordão Vibe" },
  { slug: "bracelete-couro-ouro", type: "bracelet", palette: "brown", accent: "#C9A45C", label: "Bracelete Couro & Ouro" },
  { slug: "oculos-de-sol-laumir-gold", type: "sunglasses", palette: "black", accent: "#C9A45C", label: "Óculos de Sol Laumir Gold" },
  { slug: "relogio-chrono-noir", type: "watch", palette: "black", accent: "#C9A45C", label: "Relógio Chrono Noir" },
  { slug: "bone-signature-gold", type: "cap", palette: "black", accent: "#C9A45C", label: "Boné Signature Gold" },
  { slug: "mochila-urban-premium", type: "backpack", palette: "graphite", accent: "#C9A45C", label: "Mochila Urban Premium" },
  { slug: "cinto-couro-premium", type: "belt", palette: "brown", accent: "#C9A45C", label: "Cinto Couro Premium" },
  { slug: "carteira-slim-leather", type: "wallet", palette: "brown", accent: "#C9A45C", label: "Carteira Slim Leather" },
  { slug: "tenis-street-luxe", type: "sneaker", palette: "cream", accent: "#A98B4F", label: "Tênis Street Luxe" },
  { slug: "bolsa-transversal-urban", type: "bag", palette: "black", accent: "#C9A45C", label: "Bolsa Transversal Urban" },
];

for (const product of products) {
  const p = palettes[product.palette];
  const shape = shapes[product.type];
  if (!shape) throw new Error(`Forma desconhecida: ${product.type}`);

  const main = frame(`<g>${shape(p)}</g>`, { label: product.label, accent: product.accent });
  fs.writeFileSync(path.join(OUT, `${product.slug}.svg`), main);

  const detail = frame(
    `<g transform="translate(450 560) scale(1.55) translate(-450 -560)">${shape(p)}</g>
     <rect x="28" y="28" width="844" height="1069" fill="#080808" opacity=".18"/>`,
    { label: `${product.label} — detalhe`, accent: product.accent }
  );
  fs.writeFileSync(path.join(OUT, `${product.slug}-detail.svg`), detail);
}

// Selo / monograma da marca
const mark = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 240 240">
  <rect width="240" height="240" fill="#101010"/>
  <rect x="12" y="12" width="216" height="216" fill="none" stroke="#C9A45C" stroke-opacity=".65"/>
  <text x="120" y="132" text-anchor="middle" fill="#C9A45C" font-family="Georgia, serif" font-size="66" letter-spacing="5">JLM</text>
  <text x="120" y="164" text-anchor="middle" fill="#F5F0E7" font-family="Arial, sans-serif" font-size="8" letter-spacing="4" opacity=".65">ESTILO QUE IMPÕE PRESENÇA</text>
</svg>`;
fs.writeFileSync(path.join(OUT, "..", "brand", "jlm-mark.svg"), mark);

console.log(`Geradas ${products.length * 2} imagens de produto em ${OUT}`);
