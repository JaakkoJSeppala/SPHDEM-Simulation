alert('JS loaded!');
console.log('JS loaded!');
// ...existing code from <script>...</script> in index.html...
const canvas = document.getElementById('canvas');
const ctx = canvas.getContext('2d');
const paramForm = document.getElementById('paramForm');
const freqInput = document.getElementById('freqInput');
const damperInput = document.getElementById('damperInput');
const timeInput = document.getElementById('timeInput');
const progressBar = document.getElementById('progressBar');
const modelInfo = document.getElementById('modelInfo');
const simInfo = document.getElementById('simInfo');
const statusDiv = document.getElementById('status');
const tankWidthSlider = document.getElementById('tankWidthSlider');
const tankHeightSlider = document.getElementById('tankHeightSlider');
const tankWidthValue = document.getElementById('tankWidthValue');
const tankHeightValue = document.getElementById('tankHeightValue');
let running = false;

let tankWidth_m = parseFloat(tankWidthSlider.value);
let tankHeight_m = parseFloat(tankHeightSlider.value);

tankWidthSlider.oninput = () => {
  tankWidth_m = parseFloat(tankWidthSlider.value);
  tankWidthValue.textContent = tankWidthSlider.value;
  drawTank();
};
tankHeightSlider.oninput = () => {
  tankHeight_m = parseFloat(tankHeightSlider.value);
  tankHeightValue.textContent = tankHeightSlider.value;
  drawTank();
};


function simToCanvas(x, y, params) {
  // Skaalaa simulaation koordinaatit canvasille
  const px = params.left + (x / params.tankWidth_m) * params.width;
  const py = params.top + params.height - (y / params.tankHeight_m) * params.height;
  return [px, py];
}

function drawTank(ctx, params) {
  // Tankin ääriviivat
  ctx.strokeStyle = params.color || '#117733';
  ctx.lineWidth = 3;
  ctx.strokeRect(params.left, params.top, params.width, params.height);
  // Vesitäyttö (50%)
  ctx.fillStyle = '#66B2FF';
  ctx.fillRect(params.left, params.top + params.height/2, params.width, params.height/2);
}

function drawDamper(ctx, params) {
  ctx.fillStyle = params.color || '#CC6677';
  ctx.fillRect(params.left, params.top + params.height - params.damperHeight, params.width, params.damperHeight);
}

function drawParticles(ctx, particles, params, color, radiusPx=2) {
  ctx.fillStyle = color;
  for (const p of particles) {
    const [px, py] = simToCanvas(p.x, p.y, params);
    ctx.beginPath();
    ctx.arc(px, py, radiusPx, 0, 2 * Math.PI);
    ctx.fill();
  }
}

function drawAll(state) {
  ctx.clearRect(0, 0, canvas.width, canvas.height);
  // Piirtoasetukset
  const params = {
    left: 100,
    top: 50,
    width: 600,
    height: 300,
    tankWidth_m,
    tankHeight_m,
    damperHeight: 30
  };
  // Laivan siluetti
  ctx.save();
  ctx.translate(params.left + params.width / 2, params.top - 20);
  ctx.beginPath();
  ctx.moveTo(-params.width/2, 0);
  ctx.lineTo(params.width/2, 0);
  ctx.lineTo(params.width/2 * 0.7, 25);
  ctx.lineTo(-params.width/2 * 0.7, 25);
  ctx.closePath();
  ctx.fillStyle = '#444';
  ctx.globalAlpha = 0.25;
  ctx.fill();
  ctx.globalAlpha = 1.0;
  ctx.restore();
  // Tankki
  drawTank(ctx, params);
  // Damperi
  drawDamper(ctx, { ...params, color: '#CC6677' });
  // Partikkelit
  if (state && state.sphParticles) drawParticles(ctx, state.sphParticles, params, '#3399FF');
  if (state && state.demParticles) drawParticles(ctx, state.demParticles, params, '#CC6677', 4);
  // Selittävä teksti
  ctx.font = '16px sans-serif';
  ctx.fillStyle = '#117733';
  ctx.fillText('Aframax-laivan ballastitankin 1:50 poikkileikkaus', params.left + 150, params.top - 5);
  ctx.font = '13px sans-serif';
  ctx.fillStyle = '#333';
  ctx.fillText('Granulaarivaimennin pohjalla (punainen)', params.left + 150, params.top + params.height + 20);
  ctx.fillText('Vesitäyttö (sininen), tankin liike: x(t) = A·sin(2πft)', params.left + 150, params.top + params.height + 40);
}

// Korvaa drawTank-kutsut drawAll-kutsuilla
drawAll();
statusDiv.textContent = 'Valmis odottamaan simulaatiota.';

tankWidthSlider.oninput = () => {
  tankWidth_m = parseFloat(tankWidthSlider.value);
  tankWidthValue.textContent = tankWidthSlider.value;
  drawAll();
};
tankHeightSlider.oninput = () => {
  tankHeight_m = parseFloat(tankHeightSlider.value);
  tankHeightValue.textContent = tankHeightSlider.value;
  drawAll();
};

function updateInfo(state) {
  modelInfo.innerHTML = `
    <b>Research question:</b> <br>
    How effectively does a granular damper (SPH–DEM) reduce sloshing-induced loads at resonance?<br><br>
    <b>Simulation theory:</b><br>
    - <b>SPH</b>: models fluid (water) motion and pressure<br>
    - <b>DEM</b>: models damper particles (spheres at the bottom)<br>
    - <b>Tank motion</b>: x(t) = A·sin(2πft), A=0.02 m, f = tank motion frequency (Hz)<br>
    - <b>Measurements</b>: wall pressure p(t), free surface height h(t), energy Ek(t), damping ratio ζ<br>
    <br>
    <b>Model setup:</b><br>
    Tank: 0.30 m × 0.40 m (1:50 Aframax)<br>
    Fill ratio: 50%<br>
    Damper: bottom compartment, d=5 mm, mass 12%<br>
    Motion: x(t) = 0.02·sin(2π·f·t)`;
  simInfo.innerHTML = `<b>Laskennan eteneminen:</b><br>
    Aika: ${state.time.toFixed(2)} s<br>
    Askel: ${state.step}<br>
    Seinämäpaine: ${state.wallPressure.toFixed(1)} Pa<br>
    Vapaa pinta: ${state.freeSurface.toFixed(3)} m<br>
    Energia: ${state.kineticEnergy.toFixed(3)} J<br>
    Tankin siirtymä: ${state.tankDisplacement.toFixed(3)} m`;
  statusDiv.textContent = state.status === 'running' ? 'Simulaatio käynnissä...' : 'Valmis!';
  progressBar.style.width = Math.min(100, state.progress * 100) + '%';
}

async function startSimulation(params) {
  running = true;
  statusDiv.textContent = 'Käynnistetään...';
  await fetch('/api/start', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(params)
  });
  statusDiv.textContent = 'Simulaatio käynnissä...';
  loop();
}

async function loop() {
  if (!running) return;
  const res = await fetch('/api/state');
  const state = await res.json();
  drawTank(state);
  updateInfo(state);
  if (state.progress < 1) {
    setTimeout(loop, 100);
  } else {
    statusDiv.textContent = 'Simulaatio valmis!';
    running = false;
  }
}

paramForm.onsubmit = (e) => {
  alert('Form submitted!');
  console.log('Form submitted!');
  e.preventDefault();
  if (running) return;
  const params = {
    frequency: parseFloat(freqInput.value),
    damper: damperInput.checked,
    time: parseFloat(timeInput.value)
  };
  startSimulation(params);
};

drawTank();
statusDiv.textContent = 'Valmis odottamaan simulaatiota.';
