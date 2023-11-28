let _snowCanvas = function (props) {
  const FRAME_RATE = 60;

  // Create a canvas for the snow particles
  var canvas = document.createElement("canvas");
  canvas.setAttribute("id", "snowCanvas");
  canvas.setAttribute("class", "snow");
  document.body.appendChild(canvas);

  setCanvasDimensions();

  let snowColor = props.snowColor || "#a6a6a6";

  // Check, if a valid color is configured for the snow particles
  if (!checkVariables(_isColor, [snowColor], ["snow color"])) {
    return;
  }

  var ctx = canvas.getContext("2d");

  var maxSpeed = props.maxSpeed || 3.5,
    minSpeed = props.minSpeed || 0.3,
    rMax = props.rMax || 4, // max radius of snow particles
    rMin = props.rMin || 1;

  let particleAmount = props.amount || 150;

  // Check, if the given number inputs are valid
  if (
    !checkVariables(
      _isNumber,
      [maxSpeed, minSpeed, particleAmount, rMax, rMin],
      [
        "max speed 'maxSpeed'",
        "min speed 'minSpeed'",
        "amount",
        "max radius of snow 'rMax'",
        "min radius of snow 'rMin'",
      ]
    )
  ) {
    return;
  }

  function setCanvasDimensions() {
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
  }

  window.onresize = setCanvasDimensions;

  var snowParticles = [];
  for (let i = 0; i < particleAmount; i++) {
    snowParticles.push(initializeSnowParticle());
  }

  function initializeSnowParticle() {
    return {
      x: Math.random() * window.innerWidth - rMax,
      y: Math.random() * window.innerHeight - rMax,
      radius: Math.random() * (rMax - rMin) + rMin,
      velocity: Math.random() * (maxSpeed - minSpeed) + minSpeed,
      xChangeRate: Math.random() * 1.6 - 0.8,
    };
  }

  function drawFrame() {
    ctx.clearRect(0, 0, window.innerWidth, window.innerHeight);
    ctx.beginPath();

    for (let i = 0; i < snowParticles.length; i++) {
      const { x, y, radius } = snowParticles[i];

      ctx.fillStyle = snowColor;
      ctx.moveTo(x, y);
      ctx.arc(x, y, radius, 0, 2 * Math.PI);
    }

    ctx.fill();
    updateParticles();
  }

  var delta = 0;
  function updateParticles() {
    /**
     * Update the positions of the snow particles
     */

    const START_TOP = 1;
    const START_LEFT = 2;
    const START_RIGHT = 3;

    delta += 0.01;
    for (let i = 0; i < snowParticles.length; i++) {
      let particle = snowParticles[i];

      particle.y += particle.velocity;
      particle.x +=
        Math.sin(delta + particle.xChangeRate) * particle.xChangeRate;

      let outBottom = particle.y > window.innerHeight + particle.radius;
      let outRight = particle.x > window.innerWidth + particle.radius;
      let outLeft = particle.x < -particle.radius;

      let isOffScreen = outBottom || outRight || outLeft;

      if (isOffScreen) {
        // Snow runs off screen, redefine the particle;
        let updatedParticleProperties = {
          ...initializeSnowParticle(),
          y: Math.random() * window.innerHeight,
        };

        var randomStartPostion = Math.ceil(Math.random() * 3);
        switch (randomStartPostion) {
          case START_TOP:
            updatedParticleProperties.x = Math.random() * window.innerWidth;
            updatedParticleProperties.y = -rMax;
            break;
          case START_LEFT:
            updatedParticleProperties.x = -rMax;
            break;
          case START_RIGHT:
            updatedParticleProperties.x = window.innerWidth + rMax;
            break;
        }

        snowParticles[i] = updatedParticleProperties;
      }
    }
  }

  setInterval(drawFrame, 1000 / FRAME_RATE);
};

let _isColor = function (color) {
  let tempElement = document.createElement("div");
  tempElement.style.background = color;

  return tempElement.style.background?.length > 0;
};

let _isNumber = (n) => {
  return !isNaN(parseFloat(n)) && isFinite(n);
};

let checkVariables = (checkFunc, variables, warningMessageArr) => {
  for (let i = 0; i < variables.length; i++) {
    if (!checkFunc(variables[i])) {
      console.warn(
        "_snowCanvas: please set a valid " + warningMessageArr[i] + "."
      );
      return false;
    }
  }
  return true;
};
