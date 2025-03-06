using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


// Мой телеграмм @icamysss . Проблема была в том, что бутылка пролетала насквозь верхний коллайдер, 
// А пропадала она потому что когда падала, ложилась на него сверху и лежала там. 

// Также можно добавить триггер выше скайколлайдера, если пролетел коллайдер, то в триггере поймать 
// Добавлен метод ограничения скорости rigidBody 
// На коллайдеры лучше добавить rigidBody, указать static , изменить определение коллизий rigidBody



public class BottleController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float throwForce;
    [SerializeField] private float maxThrowSpeed = 20f;                 // НОВАЯ ПЕРЕМЕННАЯ 
    [SerializeField] private float rotationSpeed;
    [SerializeField] private bool throwBegan;
    [SerializeField] private Transform startPos;
    [SerializeField] private float coins = 100;
    [SerializeField] private Vector2 startTouchPosition; 
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private bool canRotate;
    [SerializeField] private float timer;
    [SerializeField] private float scoreCooldown;
    [SerializeField] private float rotateStreakTime;
    [SerializeField] private int score;
    [SerializeField] private static int highScore;
    [SerializeField] private float bet;
    [SerializeField] private float StartCoins;
    [SerializeField] private bool gameOver;
    [SerializeField] private Animator animator;
    [SerializeField] private float currentCash;
    [SerializeField] private float currentMultiplier = 1;
    [SerializeField] private float winTimer = 0;
    [SerializeField] private bool win;
    [SerializeField] private WinChanceManager winChanceManager;
    [SerializeField] private GameObject youWinPopUp;
    [SerializeField] private bool glassBottle;
    [SerializeField] private Sprite brokenBottle;
    [SerializeField] private float throwForceBonus;
    [SerializeField] private PhysicsMaterial2D bouncingMaterial;
    [SerializeField] private PhysicsMaterial2D defaultMaterial;

    [Header("Air Rotation Settings")]
    [SerializeField] private float airRotationMultiplierMin = 1.0f;
    [SerializeField] private float airRotationMultiplierMax = 5.0f;
    [SerializeField] private float landingRotationThreshold = 1.5f;
    [SerializeField] private float rotationCorrectionSpeed = 5f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TMP_InputField betInputfield;
    [SerializeField] private TextMeshProUGUI currentCashText;
    [SerializeField] private GameObject betButton;
    [SerializeField] private GameObject claimButton;

    [Header("GroundCheck")]
    [SerializeField] private Transform feetPos;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool onCap;

    [Header("Boost Settings")]
    [SerializeField] private float boostMultiplierMin = 0.1f;
    [SerializeField] private float boostMultiplierMax = 0.2f;
    [SerializeField] private float boostWinChancePenalty = 15f;

    [Header("Sounds")]
    [SerializeField] private GameObject fallSound;
    [SerializeField] private GameObject scoreSound;
    [SerializeField] private GameObject glassSound;


    [Header("Jump Settings")]
    [SerializeField] private int maxAirJumps = 10;
    private int jumpsCount = 0;

    [Header("Swipe Settings")]
    [SerializeField] private float verticalSwipeMultiplier = 3f;
    [SerializeField] private float maxBonusPercentage = 35f;
    [SerializeField] private float initialRotationBoost = 1.25f;
    [SerializeField] private float boostDuration = 0.3f;

    [Header("Hold Settings")]
    [SerializeField] private float minHoldMultiplier = 1.5f;
    [SerializeField] private float maxHoldMultiplier = 3f;
    [SerializeField] private float maxHoldTime = 2f;

    [Header("Horizontal Throw Settings")]
    [SerializeField] private float horizontalForceMultiplier = 1.8f;
    [SerializeField] private float maxHorizontalOffset = 2f;
    [SerializeField]
    private AnimationCurve horizontalForceCurve = new AnimationCurve(
        new Keyframe(0, 0),
        new Keyframe(1, 1)
    );

    private Vector2 _initialPosition;
    private float _initialTorqueDirection = 1f;
    private bool _firstThrow = true;

    [Header("Force Settings")]
    [SerializeField] private float minThrowForce = 3f;
    [SerializeField] private float maxThrowForce = 5f;
    [SerializeField] private float forceMultiplier = 1.6f;

    [SerializeField] private float maxSwipeLength = 100f;

    [Header("Rotation Settings")]

    private float _currentRotationSpeed = 0f;

    [SerializeField] private float rotationSpeedMultiplier = 3f; 
    [SerializeField] private float maxRotationSpeed = 500f; 

    [Header("Advanced Physics")]
    [SerializeField] private float bottleMass = 0.5f; 
    [SerializeField] private float airResistance = 0.1f; 
    [SerializeField] private float angularDrag = 0.05f; 
    private float holdStartTime;
    private float holdDuration;

    private float gravityTimer = 0f;
    private bool isGrounded;
    private bool rewardGiven = false;
    private bool multiplierLocked = false;
    private bool canFlip;
    private bool multiplierUsed;
    private bool allowAdditionalBoost = false;

    private float sidewaysTimer;
    private bool canBoost = true;
    private bool betPlaced = false;

    private float animatedMultiplier = 1f;
    private float targetMultiplier = 3f;
    private Coroutine multiplierAnimationCoroutine;

    private float previousRotationZ;
    private float totalRotation;
    private int fullRotations;
    [SerializeField] private float rotationMultiplierIncrement = 1.0f;

    [Header("Trigger Zone Settings")]
    [SerializeField] private LayerMask triggerZoneLayer;
    [SerializeField] private float uprightThreshold = 10f;
    [SerializeField] private float stabilizationSpeed = 5f;
    private bool hasExitedZone;
    private bool inTriggerZone;
    private bool isStabilized;

    [Header("Air Throw Settings")]
    [SerializeField] private float airThrowWindow = 0.4f;
    [SerializeField] private float airThrowForceMultiplier = 0.66f;
    private float lastSwipeTime;
    private bool inAirThrowWindow;

    [Header("Multiplier Colors")]
    [SerializeField]
    private MultiplierColor[] multiplierColors = new MultiplierColor[]
{
    new MultiplierColor { threshold = 3f, color = Color.white },
    new MultiplierColor { threshold = 7f, color = new Color(0.678f, 0.847f, 0.902f) }, 
    new MultiplierColor { threshold = 10f, color = Color.blue },
    new MultiplierColor { threshold = 15f, color = new Color(0.5f, 0f, 0.5f) },
    new MultiplierColor { threshold = 20f, color = Color.yellow },
    new MultiplierColor { threshold = 25f, color = Color.red }
};


    [Header("Skybox Settings")]
    [SerializeField] private LayerMask skyboxLayer;
    [SerializeField] private PhysicsMaterial2D skyboxMaterial;


    [SerializeField] private float flipHoverTime = 0.1f;

    [Header("Trigger Settings")]
    [SerializeField] private float stabilizationTime = 0.5f;
    [SerializeField] private float victoryDelay = 2f;
    private bool _isStabilizing;
    private Coroutine _stabilizationCoroutine;

    [Header("Trigger Settings")]
    [SerializeField] private float postTriggerCheckDelay = 1f;
    [SerializeField] private float victoryCheckDuration = 2f;

    [Header("Force Settings")]
    [SerializeField] private float horizontalMultiplier = 1f;
    [SerializeField] private float verticalMultiplier = 1f;

    [Header("Rotation Settings")]
    [SerializeField] private float minTorque = 100f;
    [SerializeField] private float maxTorque = 500f;

    private bool isClaimed = false;
    private Coroutine autoClaimCoroutine;

    private bool sideDefeatTriggered = false;

    [System.Serializable]
    public struct MultiplierColor
    {
        public float threshold;
        public Color color;
    }
    [Header("Multiplier Colors")]

    [SerializeField] private float scaleIntensity = 1.2f;
    [SerializeField] private float colorTransitionSpeed = 2f;

    [Header("Vertical Force Settings")]
    [SerializeField] private float verticalJumpMultiplier = 1f;

    [Header("Bounce Settings")]
    [SerializeField] private int maxBounces = 3;
    [SerializeField] private int bounceCount = 0;
    [SerializeField] private float bounceForceReduction = 0.5f;
    [SerializeField] private float maxBounceForce = 8f;
    [SerializeField] private float maxVerticalBounce = 12f;
    [SerializeField] private float sidewaysBounceMultiplier = 0.3f;
    [SerializeField] private float velocityDamping = 0.7f;


    [Header("New Victory Settings")]
    [SerializeField] private float victoryRotationThreshold = 40f;
    [SerializeField] private float maxRotationDelta = 0.3f; 
    [SerializeField] private float throwCooldownTime = 1.5f;

    private bool victoryCheckActive;
    private bool throwCooldown;
    private bool victoryAchieved;


    [Header("Smooth Rotation Settings")]
    [SerializeField] private float initialRotationSpeed = 200f;
    [SerializeField] private float targetRotationSpeed = 800f;
    [SerializeField] private float rotationRampDuration = 1f;
    private float currentRotationSpeed;
    private bool isSpeedRamping;
    [Header("Loss Settings")]
    [SerializeField] private float respawnDelay = 2f;
    private bool isTriggerAfterLoss;
    private Collider2D bottleCollider;

    [Header("Slot Integration Settings")]
    [SerializeField] private bool virtualLinesActive;

    [SerializeField] private float rotationForLine = 360f;

    private bool _mainLineActive;
    private bool _bonusLineActive;
    private bool _scatterHit;

    [Header("Horizontal Movement Settings")]
    [SerializeField] private float maxSideForce = 3f;
    [SerializeField] private float sideForceMultiplier = 1.8f;
    [SerializeField] private float airControl = 0.4f;
    [SerializeField]
    private AnimationCurve sideForceCurve = new AnimationCurve(
        new Keyframe(0, 0),
        new Keyframe(1, 1)
    );
    private float _initialBottleAngle;
    private const float AngleThreshold = 30f;

    private float _lastGroundTouchTime;
    private Coroutine _lossCheckCoroutine;
    private bool _isCheckingLoss;
    
    
    // ------------------- Ограничение скорости ---------- 
    private void ClampBottleVelocity()
    {
        if (rb.linearVelocity.magnitude > maxThrowSpeed)
        {
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxThrowSpeed);
        }
    }
    // ------------------------------------------------------
    
    
    private void Start()
    {

        _initialPosition = startPos.position;
        _initialBottleAngle = NormalizeAngle(transform.eulerAngles.z);
        rb = GetComponent<Rigidbody2D>();
        bottleCollider = GetComponent<Collider2D>();
        if (bottleCollider == null)
        {
            bottleCollider = GetComponent<Collider2D>();
            Debug.LogWarning("BottleCollider not assigned - using GetComponent");
        }
        rb.mass = bottleMass;
        rb.linearDamping = airResistance;
        rb.angularDamping = 0.02f; 

        defaultMaterial.friction = 0.4f; 

        coins = PlayerPrefs.GetFloat("coins", StartCoins);
        coinsText.text = coins.ToString("F2");

        skyboxMaterial = new PhysicsMaterial2D
        {
            bounciness = 0f,
            friction = 1f
        };

        bottleCollider = GetComponent<Collider2D>();

        hasExitedZone = false;

        scoreText.text = "";
        if (winChanceManager == null)
        {
            Debug.LogError("WinChanceManager is not assigned!");
        }
        else
        {
            Debug.Log("WinChanceManager is assigned correctly.");
        }
    }

    private void ApplyRotationForce(float swipePower)
    {
        float torque = swipePower * rotationSpeed * Time.fixedDeltaTime * 2;
        rb.AddTorque(torque, ForceMode2D.Impulse);
    }

    private IEnumerator DoubleCheckBounceDefeat()
    {
        yield return new WaitForSeconds(0.3f);

        if (bounceCount >= maxBounces && !gameOver)
        {
            HandleLoss("Too many bounces!");
        }
    }
    private void ApplyBounceEffects(Collision2D collision)
    {
        if (!throwBegan || bounceCount >= maxBounces || gameOver) return;
        if (bounceCount >= maxBounces)
        {
            StartCoroutine(DoubleCheckBounceDefeat());
        }
        float reductionFactor = Mathf.Pow(bounceForceReduction, bounceCount);
        float impactForce = collision.relativeVelocity.magnitude * reductionFactor;

        float bounceForce = Mathf.Min(impactForce, maxBounceForce);

        Vector2 bounceDirection = collision.contacts[0].normal.normalized;
        rb.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);

        if (bounceCount > 0)
        {
            Vector2 newVelocity = rb.linearVelocity;
            newVelocity.y *= Mathf.Pow(0.5f, bounceCount);
            rb.linearVelocity = newVelocity;
        }

        rb.linearVelocity *= velocityDamping;

        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxVerticalBounce);

        bounceCount++;

        if (bounceCount >= maxBounces)
        {
            rb.linearVelocity *= 0.3f;
            rb.angularVelocity *= 0.5f;
            HandleLoss("Too many bounces");
        }

        StartCoroutine(PlayBounceEffect());

        if (bounceCount <= maxBounces)
        {
            Instantiate(fallSound);
        }
    }

    private IEnumerator PlayBounceEffect()
    {
        Vector3 originalScale = transform.localScale;

        float duration = 0.2f; 
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float squashY = Mathf.Lerp(1f, 0.9f, Mathf.Sin(t * Mathf.PI)); 
            float stretchX = Mathf.Lerp(1f, 1.1f, Mathf.Sin(t * Mathf.PI)); 

            // ��������� ����� �������
            transform.localScale = new Vector3(
                originalScale.x * stretchX,
                originalScale.y * squashY,
                originalScale.z
            );

            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float squashY = Mathf.Lerp(0.9f, 1f, Mathf.Sin(t * Mathf.PI)); 
            float stretchX = Mathf.Lerp(1.1f, 1f, Mathf.Sin(t * Mathf.PI)); 

            transform.localScale = new Vector3(
                originalScale.x * stretchX,
                originalScale.y * squashY,
                originalScale.z
            );

            yield return null;
        }

        transform.localScale = originalScale;
    }
 

    private IEnumerator ResetSquash()
    {
        yield return new WaitForSeconds(0.1f);
        transform.localScale = Vector3.one;
    }

    public bool PlaceBet(int value)
    {
        if (coins < value) return false;
        bet = value;
        betPlaced = true;
        coins -= bet;
        coinsText.text = coins.ToString("F2");
        PlayerPrefs.SetFloat("coins", coins);
        PlayerPrefs.Save();
        return true;
    }

    public void RestartScene()
    {
        CancelInvoke();
        StopAllCoroutines();

        if (bottleCollider != null)
        {
            bottleCollider.isTrigger = false;
            bottleCollider.transform.localScale = Vector3.one;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReceiveCoins(float value)
    {
        coins += value;
        coinsText.text = coins.ToString("F2");

        PlayerPrefs.SetFloat("coins", coins);
        PlayerPrefs.Save();
    }
    public void AddCoins()
    {
        coins += 10;
        PlayerPrefs.SetFloat("coins", 1000);
        PlayerPrefs.Save();
    }
    public void ClaimReward()
    {
        if (autoClaimCoroutine != null)
        {
            StopCoroutine(autoClaimCoroutine);
            autoClaimCoroutine = null;
        }

        ReceiveCoins(currentCash);

        currentCash = 0;
        currentMultiplier = 1;
        targetMultiplier = 1f;
        animatedMultiplier = 1f;

        currentCashText.text = currentCash.ToString("F2");
        scoreText.text = "";
        youWinPopUp.SetActive(false);

        if (winChanceManager != null)
        {
            winChanceManager.GenerateNewMultiplier();
        }

        SceneManager.LoadScene(0);
    }
    private void CheckAndStabilizeRotation()
    {
        if (!hasExitedZone) return;

        StabilizeBottle();
    }
    [SerializeField] private bool hasTouchedGround;
    private void StabilizeBottle()
    {
        float currentAngle = NormalizeAngle(transform.eulerAngles.z);
        float targetAngle = (currentAngle > 180f) ? 360f : 0f;

        float newRotation = Mathf.LerpAngle(
            currentAngle,
            targetAngle,
            Time.deltaTime * 15f
        );

        rb.MoveRotation(newRotation);

        if (Mathf.Abs(newRotation - targetAngle) < 0.5f)
        {
            isStabilized = true;
            rb.angularVelocity = 0f;
        }
    }

    private void Update()
    {

        if (!isSpeedRamping && Mathf.Abs(rb.angularVelocity) > targetRotationSpeed)
        {
            rb.angularVelocity = Mathf.Sign(rb.angularVelocity) * targetRotationSpeed;
        }
        if (throwBegan && !gameOver)
        {
            float clampedX = Mathf.Clamp(
                transform.position.x,
                _initialPosition.x - maxHorizontalOffset,
                _initialPosition.x + maxHorizontalOffset
            );
            transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
        }

        isGrounded = Physics2D.OverlapCircle(feetPos.position, groundCheckRadius, groundLayer);
        if (isGrounded && !hasTouchedGround)
        {
            hasTouchedGround = true;
        }
        if (inTriggerZone && !isStabilized)
        {
            CheckAndStabilizeRotation();
        }
        if (rotationSpeed > maxRotationSpeed)
        {
            rotationSpeed = maxRotationSpeed;
        }
        if (Mathf.Abs(rb.angularVelocity) > maxRotationSpeed)
        {
            rb.angularVelocity = Mathf.Sign(rb.angularVelocity) * maxRotationSpeed;
        }
        if ((isGrounded && throwBegan) || (onCap && throwBegan))
            winTimer += Time.deltaTime;

        if (!betPlaced || gameOver) return;

        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = (Vector2)Input.mousePosition;
            holdStartTime = Time.time;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!throwBegan && timer > 0.2f)
            {
                youWinPopUp.SetActive(false);

                Vector2 swipeDelta = (Vector2)Input.mousePosition - startTouchPosition;
                float swipeLength = Mathf.Clamp(swipeDelta.magnitude, 0, maxSwipeLength);
                float normalizedSwipe = swipeLength / maxSwipeLength; 

                ThrowBottle(normalizedSwipe); 
            }
            else if (throwBegan && !isGrounded)
            {
                Vector2 swipeDelta = (Vector2)Input.mousePosition - startTouchPosition;
                ApplyAirJump(swipeDelta);
            }

        }

        if (Input.GetMouseButtonUp(0) && throwBegan && timer > 0.1f && !gameOver)
        {
            if (transform.localEulerAngles.z < 70 && transform.localEulerAngles.z > 10 &&
                timer > 0.2f && rotationSpeed == 0 && score < 3)
                return;

            if (Camera.main != null)
            {
                Camera.main.GetComponent<Animator>().SetBool("Sky", true);
            }

            float swipeDistance = ((Vector2)Input.mousePosition - startTouchPosition).magnitude; // ��������
            if (swipeDistance > 100)
                rotateStreakTime += 0.2f;

            if (timer > 1.0f)
            {
                float maxChange = rotationSpeed * 0.1f * Time.deltaTime;
                float desiredRotationSpeed = rotationSpeed + Mathf.Lerp(swipeDistance / 4f, swipeDistance / 2f,
                    holdDuration / maxHoldTime);
                rotationSpeed = Mathf.MoveTowards(rotationSpeed, desiredRotationSpeed, maxChange);
            }
            else
            {
                rotationSpeed += Mathf.Lerp(swipeDistance / 4f, swipeDistance / 2f,
                    holdDuration / maxHoldTime);
            }

            rotationSpeed = Mathf.Clamp(rotationSpeed, 0f, 2000f);
        }

        if (rotateStreakTime < 0)


            if (transform.localEulerAngles.z > 180 && transform.localEulerAngles.z < 185 &&
                scoreCooldown > 0.05f && rotationSpeed > 0 && canRotate)
            {
                AddMultiplier();
                scoreCooldown = 0;
            }

        if (transform.localEulerAngles.z < 10 && transform.position.y < -1 &&
            timer > 0.8f && win && rotateStreakTime < 0.1)
            rotationSpeed = 20;

        scoreCooldown += Time.deltaTime;
        timer += Time.deltaTime;

        if (timer > 1.7f)
            rb.sharedMaterial = defaultMaterial;

        if (throwBegan && timer > 0.2f && !win)
            rb.sharedMaterial = bouncingMaterial;

        isGrounded = Physics2D.OverlapCircle(feetPos.position, groundCheckRadius, groundLayer);

        canRotate = !(isGrounded || onCap);
        if (!canRotate)
            rotationSpeed = 0;

        if (isGrounded && !gameOver)
        {
            float currentAngle = transform.localEulerAngles.z % 360;
            bool isSideways = (currentAngle > 75f && currentAngle < 105f)
                            || (currentAngle > 255f && currentAngle < 285f);

            if (isSideways && !sideDefeatTriggered)
            {
                sideDefeatTriggered = true;
                if (isSideways && !sideDefeatTriggered)
                {
                    sideDefeatTriggered = true;
                    StartCoroutine(DoubleCheckSideDefeat());
                }
            }
        }

        if (throwBegan && !isGrounded)
        {
            float currentRotationZ = transform.localEulerAngles.z;
            float deltaRotation = Mathf.DeltaAngle(previousRotationZ, currentRotationZ);
            totalRotation += Mathf.Abs(deltaRotation);

            if (totalRotation >= 360f)
            {
                fullRotations++;
                totalRotation -= 360f;
                targetMultiplier += rotationMultiplierIncrement;
                AddMultiplier();
            }

            previousRotationZ = currentRotationZ;
        }

        if (win && !isGrounded && transform.position.y < landingRotationThreshold)
        {
            RotateForLanding();
        }
        if (isGrounded && throwBegan && !_isCheckingLoss && !gameOver)
        {
            _lossCheckCoroutine = StartCoroutine(CheckLossAfterDelay(4f));
        }
    }
    private IEnumerator DoubleCheckSideDefeat()
    {
        yield return new WaitForSeconds(0.3f);

        if (CheckSidewaysDefeat() && !gameOver)
        {
            HandleLoss("Bottle fell sideways!");
        }
        else
        {
            sideDefeatTriggered = false;
        }
    }
    private void ApplyAirJump(Vector2 swipeDelta)
    {
        if (hasTouchedGround || gameOver || jumpsCount >= maxAirJumps) return;

        float swipeLength = swipeDelta.magnitude;
        if (swipeLength < maxSwipeLength * 0.1f) return;

        float swipePower = Mathf.Clamp01(swipeLength / maxSwipeLength);

        float verticalForce = Mathf.Lerp(
            minThrowForce * 0.7f,
            maxThrowForce * 3.5f,
            swipePower
        ) * verticalJumpMultiplier;

        rb.linearVelocity = new Vector2(0, verticalForce);

        _currentRotationSpeed += 80f; 
        rb.AddTorque(_currentRotationSpeed, ForceMode2D.Impulse);

        bounceCount = 0;
        jumpsCount++;
        AddMultiplier(true);
        
        // -------------
        ClampBottleVelocity();
        // -------------
    }



    private IEnumerator AirThrowWindowCheck()
    {
        inAirThrowWindow = true;
        yield return new WaitForSeconds(airThrowWindow);
        inAirThrowWindow = false;
    }

    private IEnumerator CheckLossAfterDelay(float delay)
    {
        _isCheckingLoss = true;
        float startTime = Time.time;

        while (Time.time - startTime < delay)
        {
            if (!isGrounded || gameOver)
            {
                _isCheckingLoss = false;
                yield break;
            }
            yield return null;
        }

        if (isGrounded && !gameOver)
        {
            yield return new WaitForSeconds(1f);
            CheckFinalResult();
        }
        _isCheckingLoss = false;
    }
    private IEnumerator DelayedSideDefeat()
    {
        yield return new WaitForSeconds(0.2f);
        bool isSideways = (transform.localEulerAngles.z > 80 && transform.localEulerAngles.z < 100) ||
                          (transform.localEulerAngles.z > 260 && transform.localEulerAngles.z < 280);
        if (isGrounded && isSideways)
        {
            win = false;
            gameOver = true;
            canRotate = false;
            rb.linearVelocity = Vector2.zero;

            youWinPopUp.SetActive(true);
            youWinPopUp.GetComponent<TextMeshProUGUI>().text = "you lose!";
            claimButton.SetActive(false);
            yield return new WaitForSeconds(2f);
            RestartScene();
        }
    }

    private IEnumerator SmoothLandingAndWin()
    {
        rb.angularVelocity = 0f;
        gameOver = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        yield return StartCoroutine(SmoothAlignToAngle(0f, 0.3f));

        rewardGiven = true;

        youWinPopUp.SetActive(true);
        youWinPopUp.GetComponent<TextMeshProUGUI>().text = "YOU WIN!";
        currentCash = Mathf.Round(bet * targetMultiplier * 100) / 100;
        currentCashText.text = currentCash.ToString("F2");

        autoClaimCoroutine = StartCoroutine(AutoClaimAfterDelay(10f));
    }

    private IEnumerator AutoClaimAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!isClaimed)
        {
            ClaimReward();
            betButton.SetActive(true);
            claimButton.SetActive(false);
            betPlaced = false;
        }
    }

    private void HandleLanding()
    {
        canRotate = false;
        rb.angularVelocity = 0f;
        rotationSpeed = 0f;

        if (win)
        {
            StartCoroutine(SmoothAlignToAngle(0f, 0.3f));
        }
        else
        {
            rb.angularVelocity = UnityEngine.Random.Range(200f, 400f);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (winChanceManager == null)
        {
            return;
        }

        if (((1 << other.gameObject.layer) & triggerZoneLayer) != 0 && win)
        {
            inTriggerZone = true;

            Debug.Log("Entered trigger zone. Win condition is true.");

            if (hasExitedZone)
            {
                isStabilized = false;
            }
        }
    }
    private IEnumerator StabilizationProcess()
    {
        _isStabilizing = true;
        float timer = 0f;

        while (timer < stabilizationTime && !gameOver)
        {
            if (!IsStablePosition())
            {
                _isStabilizing = false;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        LockBottlePosition();

        timer = 0f;
        while (timer < victoryDelay)
        {
            if (!IsPerfectlyUpright())
            {
                ReleaseBottle();
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        HandleVictory();
    }
    private bool IsStablePosition()
    {
        float angle = Mathf.Abs(transform.eulerAngles.z % 360);
        return (angle <= 45f || angle >= 315f) &&
               rb.angularVelocity < 45f &&
               rb.linearVelocity.magnitude < 1f;
    }
    private bool IsPerfectlyUpright()
    {
        float angle = Mathf.Abs(transform.eulerAngles.z % 360);
        return (angle <= 1f || angle >= 359f) &&
               rb.angularVelocity < 5f &&
               rb.linearVelocity.magnitude < 0.1f;
    }
    private void LockBottlePosition()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = true;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    private void ReleaseBottle()
    {
        rb.isKinematic = false;
        _isStabilizing = false;
        rb.AddForce(new Vector2(
            UnityEngine.Random.Range(-0.5f, 0.5f),
            UnityEngine.Random.Range(0.5f, 1.5f)
        ), ForceMode2D.Impulse);
    }
    private IEnumerator PostTriggerCheckRoutine()
    {
        yield return new WaitForSeconds(postTriggerCheckDelay);

        float checkTimer = 0f;
        while (checkTimer < victoryCheckDuration && !gameOver && !inTriggerZone)
        {
            if (IsUpright() && rb.linearVelocity.magnitude < 0.1f)
            {
                HandleVictory();
                yield break;
            }
            checkTimer += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & triggerZoneLayer) != 0)
        {
            inTriggerZone = false;
            isStabilized = false;

            hasExitedZone = true;
        }
    }
    private IEnumerator VictoryCheckRoutine()
    {
        float checkTimer = 0f;
        while (inTriggerZone && checkTimer < victoryCheckDuration && !gameOver)
        {
            if (IsUpright() && rb.linearVelocity.magnitude < 0.3f)
            {
                HandleVictory();
                yield break;
            }
            checkTimer += Time.deltaTime;
            yield return null;
        }
    }
    private bool IsUpright()
    {
        float angle = transform.eulerAngles.z % 360;
        bool angleCheck = angle <= 45f || angle >= 315f;
        bool physicsCheck = rb.linearVelocity.magnitude < 0.5f && Mathf.Abs(rb.angularVelocity) < 15f;
        return angleCheck && physicsCheck;
    }
    private void HandleVictory()
    {
        victoryAchieved = true;
        gameOver = true;
        rb.isKinematic = true;

        StartCoroutine(VictoryAnimation());

        youWinPopUp.SetActive(true);
        currentCash = Mathf.Round(bet * currentMultiplier * 100) / 100;
        currentCashText.text = currentCash.ToString("F2");
        claimButton.SetActive(true);
    }
    private IEnumerator VictoryAnimation()
    {
        float duration = 1f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = startScale * 1.2f;

        while (duration > 0)
        {
            transform.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                Mathf.PingPong(Time.time, 0.5f)
            );
            duration -= Time.deltaTime;
            yield return null;
        }
        transform.localScale = startScale;
    }

    private void FixedUpdate()
    {
        // -------------
        ClampBottleVelocity();
        // -------------
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
      
        if (isTriggerAfterLoss)
        {
            rb.gravityScale = 3f;
            rb.AddForce(Vector2.down * 10f, ForceMode2D.Force);
            transform.localScale *= 0.98f;
            return;
        }

        if (!throwBegan || gameOver) return;

        rb.linearDamping = isGrounded ?
            Mathf.Lerp(0.5f, 2f, rb.linearVelocity.magnitude / maxVerticalBounce) :
            airResistance;

        Vector2 clampedVelocity = rb.linearVelocity;
        clampedVelocity.x = Mathf.Clamp(clampedVelocity.x, -maxHorizontalOffset * 2, maxHorizontalOffset * 2);
        clampedVelocity.y = Mathf.Min(clampedVelocity.y, maxVerticalBounce);
        rb.linearVelocity = clampedVelocity;

        if (Mathf.Abs(rb.angularVelocity) > targetRotationSpeed * 0.8f)
        {
            rb.angularVelocity *= 0.97f;
        }
    }

    private void HandleLoss(string reason)
    {
        if (gameOver || win) return;

        Debug.Log($"HandleLoss called. Reason: {reason}");

        winChanceManager.AdjustChanceAfterThrow(false, bet);
        StartCoroutine(FinalDefeatCheck(reason));
    }

    private IEnumerator FinalDefeatCheck(string reason)
    {
        yield return new WaitForSeconds(0.5f);

        if (!IsDefeatConditionStillValid() || win)
        {
            Debug.Log("Defeat condition no longer valid - canceling loss");
            yield break;
        }

        yield return new WaitForSeconds(0.3f);

        if (!IsDefeatConditionStillValid() || win)
        {
            Debug.Log("Defeat condition canceled after second check");
            yield break;
        }

        Debug.Log($"Final defeat confirmed: {reason}");

        win = false;
        gameOver = true;
        canRotate = false;
        rb.linearVelocity = Vector2.zero;

        if (bottleCollider != null)
        {
            bottleCollider.isTrigger = true;
        }

        youWinPopUp.SetActive(true);
        youWinPopUp.GetComponent<TextMeshProUGUI>().text = "YOU LOSE!";
        claimButton.SetActive(false);

        StartCoroutine(DelayedRestart(2f));
    }

    private bool IsDefeatConditionStillValid()
    {
        bool isSideways = CheckSidewaysDefeat();
        bool isBounceLimit = bounceCount >= maxBounces;
        bool isVelocityTooHigh = rb.linearVelocity.magnitude > 5f;

        return isSideways || isBounceLimit || isVelocityTooHigh;
    }

    private bool CheckSidewaysDefeat()
    {
        if (!isGrounded) return false;

        float currentAngle = transform.localEulerAngles.z % 360;
        return (currentAngle > 75f && currentAngle < 105f) ||
               (currentAngle > 255f && currentAngle < 285f);
    }


    private IEnumerator DelayedRestart(float delay)
    {
        yield return new WaitForSeconds(delay);
        RestartScene();
    }


    private IEnumerator ResetAfterLoss()
    {
        yield return new WaitForSeconds(1f);

        youWinPopUp.SetActive(false);
        betButton.SetActive(true);
        claimButton.SetActive(false);
        betPlaced = false;
        gameOver = false;

        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;


        ResetForNextThrow();
    }

    public void OnBetButtonClicked()
    {
        if (PlaceBet(Convert.ToInt32(betInputfield.text)))
        {
            winChanceManager.CheckForDeviation();

            currentMultiplier = winChanceManager.CurrentMultiplier;
            float currentChance = winChanceManager.CurrentWinChance;

            currentChance = Mathf.Clamp(currentChance,
                winChanceManager.minWinChance,
                winChanceManager.maxWinChance);

            win = winChanceManager.CalculateWinResult();
            /*
                        Debug.Log($"Bottle throw params: Chance={currentChance}%, " +
                                 $"Multiplier={currentMultiplier}x, RTP={currentChance * currentMultiplier}%");

                        // ������������ ������� �������
                        currentCash = Mathf.Round(bet * targetMultiplier * 100) / 100;
                        currentCashText.text = currentCash.ToString("F2");
            */
            scoreText.text = "";
            scoreText.gameObject.SetActive(true);

            transform.position = startPos.position;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rotationSpeed = 0;
            bounceCount = 0;
            jumpsCount = 0;
            gameOver = false;

            betButton.SetActive(false);
            claimButton.SetActive(true);
        }
    }
    private IEnumerator ThrowCooldownRoutine()
    {
        throwCooldown = true;
        yield return new WaitForSeconds(throwCooldownTime);
        throwCooldown = false;
    }

    private void ThrowBottle(float normalizedSwipe)
    {
        winChanceManager.GeneratePredefinedResult();

        normalizedSwipe = Mathf.Clamp(normalizedSwipe, 0.2f, 1f);

        Vector2 swipeVector = (Vector2)Input.mousePosition - startTouchPosition;

        if (_firstThrow)
        {
            Vector2 swipeDirection = swipeVector.normalized;
            _initialTorqueDirection = -Mathf.Sign(swipeDirection.x);
            _firstThrow = false;
        }

        float verticalForce = Mathf.Lerp(minThrowForce, maxThrowForce, normalizedSwipe) * verticalMultiplier;

        rb.AddForce(new Vector2(0, verticalForce), ForceMode2D.Impulse);

        // -------------
        ClampBottleVelocity();
        // -------------
        _currentRotationSpeed = Mathf.Lerp(minTorque, maxTorque, normalizedSwipe)
                              * _initialTorqueDirection
                              * 1.3f; 

        rb.AddTorque(_currentRotationSpeed, ForceMode2D.Impulse);

        throwBegan = true;
        isStabilized = false;
        StartCoroutine(ThrowCooldownRoutine());
    }

    private IEnumerator RampRotationSpeed()
    {
        float elapsed = 0f;
        float startSpeed = currentRotationSpeed;

        while (elapsed < rotationRampDuration)
        {
            elapsed += Time.deltaTime;
            currentRotationSpeed = Mathf.Lerp(startSpeed, targetRotationSpeed, elapsed / rotationRampDuration);
            yield return null;
        }

        currentRotationSpeed = targetRotationSpeed;
        isSpeedRamping = false;
    }
    private Vector2 CalculateThrowForce(float normalizedSwipe)
    {
        return new Vector2(
            Mathf.Lerp(minThrowForce, maxThrowForce, normalizedSwipe) * horizontalMultiplier,
            Mathf.Lerp(minThrowForce, maxThrowForce, normalizedSwipe) * verticalMultiplier
        );
    }
    private float CalculateTorque(float normalizedSwipe)
    {
        return Mathf.Lerp(minTorque, maxTorque, normalizedSwipe) *
               Mathf.Sign(UnityEngine.Random.Range(-1f, 1f));
    }
    private IEnumerator ResetRotationBoost()
    {
        yield return new WaitForSeconds(boostDuration);
        rotationSpeed /= initialRotationBoost;
    }

    private void ApplyAdditionalBoost()
    {
        if (gameOver || jumpsCount >= maxAirJumps || win || !inAirThrowWindow) return;

        Vector2 endTouchPosition = Input.mousePosition;
        float swipeDistance = (endTouchPosition - startTouchPosition).magnitude * 1.9f;

        if (swipeDistance < 700) return;

        float holdMultiplier = Mathf.Lerp(minHoldMultiplier, maxHoldMultiplier,
            Mathf.Clamp01(holdDuration / maxHoldTime));

        float swipeMultiplier = Mathf.Clamp01(swipeDistance / 1000f);
        float totalForceMultiplier = holdMultiplier * (1 + swipeMultiplier);

        float newThrowForce = (swipeDistance / 15f + throwForceBonus) * totalForceMultiplier * 2f;

        float newRotationSpeed = Mathf.Max(500f, 700f - (swipeDistance / 1500f)) * initialRotationBoost;
        rotationSpeed = Mathf.Abs(newRotationSpeed * 2);

        rb.linearVelocity = new Vector2(0f, newThrowForce * airThrowForceMultiplier);
        StartCoroutine(ResetRotationBoost());

        canRotate = true;
        winTimer = 0;
        timer = 0;

        float multiplierIncrement = UnityEngine.Random.Range(airRotationMultiplierMin, airRotationMultiplierMax);
        targetMultiplier += multiplierIncrement;
        targetMultiplier = Mathf.Round(targetMultiplier * 100) / 100;

        if (multiplierAnimationCoroutine != null)
            StopCoroutine(multiplierAnimationCoroutine);

        multiplierAnimationCoroutine = StartCoroutine(AnimateMultiplier());
        currentCash = Mathf.Floor(bet * targetMultiplier);
        currentCashText.text = currentCash.ToString("F2");
        scoreText.GetComponent<Animator>().SetTrigger("score");
        Instantiate(scoreSound);

        Debug.Log($"Applying pre-calculated win: {win}");

        jumpsCount++;
        StartCoroutine(ResetBoostCooldown());

        holdDuration = 0f;
        holdStartTime = 0f;

        lastSwipeTime = Time.time;
        StartCoroutine(AirThrowWindowCheck());
    }

    private IEnumerator ResetBoostCooldown()
    {
        canBoost = false;
        yield return new WaitForSeconds(0.3f);
        canBoost = true;
    }

    private IEnumerator DetermineWinLoseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (gameOver) yield break;

        if (!win || (transform.localEulerAngles.z > 268 && transform.localEulerAngles.z < 272 ||
            transform.localEulerAngles.z > 87 && transform.localEulerAngles.z < 93))
        {
            youWinPopUp.SetActive(true);
            youWinPopUp.GetComponent<TextMeshProUGUI>().text = "you lose!";
            Invoke(nameof(RestartScene), 1f);
            gameOver = true;
            canFlip = false;
        }
        else
        {
            if (!rewardGiven)
            {
                youWinPopUp.SetActive(true);
                youWinPopUp.GetComponent<TextMeshProUGUI>().text = "you win!";
                currentCashText.text = currentCash.ToString("F2");
                rewardGiven = true;

                ResetForNextThrow();
                canFlip = false;
            }
        }
    }
    private void UpdateRTPBalance()
    {
        float expectedRTP = (winChanceManager.CurrentWinChance / 100f) * currentMultiplier;

        if (expectedRTP < 0.9f)
        {
            winChanceManager.AdjustChance(true); 
        }
        else if (expectedRTP > 1.1f)
        {
            winChanceManager.AdjustChance(false); 
        }
    }
    public void AddMultiplier(bool isAirJump = false)
    {
        if ((!isAirJump) || gameOver) return;

        float baseMultiplier = winChanceManager.CurrentMultiplier;

        float rotationBonus = fullRotations * 0.5f;

        float difficultyBonus = inTriggerZone ? 2.0f : 1.0f;

        currentMultiplier = (baseMultiplier + rotationBonus) * difficultyBonus;

        currentMultiplier = Mathf.Min(currentMultiplier, winChanceManager.maxMultiplier);

        if (winChanceManager != null)
        {
            winChanceManager.DecreaseWinChancePerRotation();
        }

        if (multiplierAnimationCoroutine != null)
        {
            StopCoroutine(multiplierAnimationCoroutine);
        }
        multiplierAnimationCoroutine = StartCoroutine(AnimateMultiplier());

        currentCash = Mathf.Round(bet * targetMultiplier * 100) / 100;
        currentCashText.text = currentCash.ToString("F2");

        scoreText.GetComponent<Animator>().SetTrigger("score");
        Instantiate(scoreSound);

        multiplierUsed = !isAirJump;
    }
    private Color GetColorForMultiplier(float multiplier)
    {
        if (multiplierColors.Length == 0) return Color.white;

        if (multiplier >= multiplierColors[multiplierColors.Length - 1].threshold)
            return multiplierColors[multiplierColors.Length - 1].color;

        for (int i = 0; i < multiplierColors.Length - 1; i++)
        {
            if (multiplier >= multiplierColors[i].threshold &&
                multiplier < multiplierColors[i + 1].threshold)
            {
                float t = (multiplier - multiplierColors[i].threshold) /
                         (multiplierColors[i + 1].threshold - multiplierColors[i].threshold);
                return Color.Lerp(
                    multiplierColors[i].color,
                    multiplierColors[i + 1].color,
                    Mathf.Clamp01(t)
                );
            }
        }

        return multiplierColors[0].color;
    }
    private IEnumerator AnimateMultiplier()
    {
        float startValue = animatedMultiplier;
        float endValue = targetMultiplier;
        float duration = 0.5f;
        float elapsed = 0f;

        Vector3 originalScale = scoreText.transform.localScale;
        Color startColor = scoreText.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            animatedMultiplier = Mathf.Lerp(startValue, endValue, t);
            scoreText.text = "x" + animatedMultiplier.ToString("F2");

            currentCash = Mathf.Round(bet * animatedMultiplier * 100) / 100;
            currentCashText.text = currentCash.ToString("F2");

            Color targetColor = GetColorForMultiplier(animatedMultiplier);
            scoreText.color = Color.Lerp(scoreText.color, targetColor, Time.deltaTime * colorTransitionSpeed);

            float scaleFactor = Mathf.Lerp(1f, scaleIntensity, Mathf.PingPong(t * 2, 1));
            scoreText.transform.localScale = originalScale * scaleFactor;

            yield return null;
        }

        animatedMultiplier = endValue;
        scoreText.text = "x" + animatedMultiplier.ToString("F2");
        scoreText.color = GetColorForMultiplier(endValue);
        scoreText.transform.localScale = originalScale;
    }

    private void RotateForLanding()
    {
        if (win && !isGrounded && transform.position.y < landingRotationThreshold)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, 0f, Time.deltaTime * 2f);
        }
    }

    private void ResetForNextThrow()
    {
        _currentRotationSpeed = 0f;
        _firstThrow = true; 
        _initialBottleAngle = NormalizeAngle(transform.eulerAngles.z);
        hasTouchedGround = false;
        transform.position = startPos.position;
        scoreText.color = GetColorForMultiplier(1f);
        throwBegan = false;
        canRotate = false;
        timer = 0;
        winTimer = 0;
        rotationSpeed = 0;
        rb.linearVelocity = Vector2.zero;
        rb.sharedMaterial = defaultMaterial;

        transform.rotation = Quaternion.identity;
        gameOver = false;
        rewardGiven = false;
        multiplierLocked = false;


        scoreText.text = "x1.00";
        scoreText.gameObject.SetActive(false);

        bounceCount = 0;
        rb.linearDamping = airResistance;
        hasExitedZone = false;
        win = false;

        if (winChanceManager != null)
        {
            winChanceManager.GenerateNewMultiplier();
            winChanceManager.AdjustChanceAfterThrow(win, bet);
        }
    }

    public void OnClaimButtonClicked()
    {
        if (autoClaimCoroutine != null)
        {
            StopCoroutine(autoClaimCoroutine);
            autoClaimCoroutine = null;
        }

        ClaimReward();
        betButton.SetActive(true);
        claimButton.SetActive(false);
        betPlaced = false;
    }

    private IEnumerator SmoothFall(float targetAngle)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector2 startVelocity = rb.linearVelocity;
        float startAngular = rb.angularVelocity;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            rb.linearVelocity = Vector2.Lerp(startVelocity, Vector2.zero, t);
            rb.angularVelocity = Mathf.Lerp(startAngular, 0f, t);
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);

            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.rotation = targetRotation;
        gameOver = true;
    }

    private IEnumerator SmoothAlignToAngle(float targetAngle, float duration)
    {
        float startAngle = NormalizeAngle(transform.localEulerAngles.z);
        float diff = Mathf.Abs(startAngle - targetAngle);
        if (diff > 180f) 
        {
            targetAngle = (startAngle < 180f) ? 360f : 0f;
        }

        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                elapsed / duration
            );
            yield return null;
        }
        transform.rotation = targetRotation;
    }
    private void HandleSkyboxCollision(Collision2D collision)
    {
        if (!gameOver)
        {
            Debug.Log("SkyBox collision detected!");
            HandleLoss("Touched Sky Boundary");
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((skyboxLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            HandleSkyboxCollision(collision);
            return;
        }

        if (collision.gameObject.CompareTag("Ground") && !gameOver)
        {
            ApplyBounceEffects(collision);

            if (throwBegan && !rewardGiven)
            {
                if (_lossCheckCoroutine == null)
                {
                    _lossCheckCoroutine = StartCoroutine(CheckLandingResult());
                }
            }
        }
    }
    private bool IsBottleUpright()
    {
        float currentAngle = NormalizeAngle(transform.localEulerAngles.z);
        return currentAngle <= 35f || currentAngle >= 335f; 
    }
    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        return angle < 0 ? angle + 360f : angle;
    }


    private bool CheckStability()
    {
        float currentAngle = NormalizeAngle(transform.localEulerAngles.z);
        bool isUpright = currentAngle <= 35f || currentAngle >= 325f; 

        bool isVelocityLow = rb.linearVelocity.magnitude < 0.8f; 
        bool isRotationSlow = Mathf.Abs(rb.angularVelocity) < 60f; 

        return isUpright && isVelocityLow && isRotationSlow;
    }
    private bool IsWithinAngleThreshold()
    {
        float currentAngle = NormalizeAngle(transform.eulerAngles.z);
        float minAngle = NormalizeAngle(_initialBottleAngle - AngleThreshold);
        float maxAngle = NormalizeAngle(_initialBottleAngle + AngleThreshold);

        if (minAngle > maxAngle)
        {
            return currentAngle >= minAngle || currentAngle <= maxAngle;
        }

        return currentAngle >= minAngle && currentAngle <= maxAngle;
    }
    private IEnumerator CheckLandingResult()
    {
        yield return new WaitForSeconds(7f);
        while (bounceCount < 3 && !gameOver)
        {
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        float currentAngle = NormalizeAngle(transform.localEulerAngles.z);
        bool isUpright = currentAngle <= 35f || currentAngle >= 325f; 

        bool isVelocityLow = rb.linearVelocity.magnitude < 0.5f; 
        bool isRotationSlow = Mathf.Abs(rb.angularVelocity) < 30f; 

        if (isUpright && isVelocityLow && isRotationSlow)
        {
            HandleWin();
        }
        else
        {
            HandleLoss("Bottle didn't stabilize!");
        }
    }
    private void CheckWinImmediately()
    {
        float currentAngle = transform.localEulerAngles.z % 360;
        bool isWinPosition = (currentAngle >= 350f || currentAngle <= 10f);

        if (isWinPosition)
        {
            HandleWin();
        }
    }
    private bool CheckFinalStability()
    {
        if (!IsWithinAngleThreshold()) return false;

        bool isVelocityLow = rb.linearVelocity.magnitude < 0.3f;
        bool isRotationSlow = Mathf.Abs(rb.angularVelocity) < 15f;

        return isVelocityLow && isRotationSlow;
    }
    private void CheckFinalResult()
    {
        StartCoroutine(CheckFinalResultWithDelay());
    }
    private IEnumerator CheckFinalResultWithDelay()
    {
        yield return new WaitForSeconds(7f);

        if (IsBottleUpright())
        {
            HandleWin();
            yield break;
        }

        float angle = NormalizeAngle(transform.localEulerAngles.z);
        bool isSideways = (angle > 80f && angle < 100f) ||
                         (angle > 260f && angle < 280f);

        if (isSideways)
        {
            HandleLoss("Bottle landed sideways!");
        }
        else
        {
            HandleLoss("Bottle didn't land properly!");
        }
    }

    private IEnumerator CheckFinalResult(bool isWinPosition)
    {
        yield return new WaitForSeconds(5f);

        float stabilizedAngle = transform.localEulerAngles.z % 360;
        bool stabilizedWin = (stabilizedAngle >= 350f || stabilizedAngle <= 10f);

        if (stabilizedWin && isWinPosition)
        {
            HandleWin();
        }
        else
        {
            StartCoroutine(CheckLandingResult());
        }
    }
    private void HandleWin()
    {
        if (gameOver) return;

        win = true;
        gameOver = true;
        canRotate = false;
        winChanceManager.AdjustChanceAfterThrow(true, bet);
 
        currentCash = bet * currentMultiplier;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        StartCoroutine(SmoothAlignToAngle(0f, 0.5f));

        currentCash = Mathf.Round(bet * targetMultiplier * 100) / 100;
        currentCashText.text = currentCash.ToString("F2");

        youWinPopUp.SetActive(true);
        youWinPopUp.GetComponent<TextMeshProUGUI>().text = "YOU WIN!";
        claimButton.SetActive(true);

        Debug.Log($"Player won! Bet: {bet}, Multiplier: {targetMultiplier}, Winnings: {currentCash}");
    }
}