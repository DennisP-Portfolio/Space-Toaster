using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private SpeedUpDebug speedUpDebug;
    private Animator anim;
    Gun[] guns;

    private float moveSpeed = 3;
    private float inputX = 0f;
    private float inputY = 0f;

    private Rigidbody rb;

    #region life
    [SerializeField] BarController healthBar;

    [SerializeField] public int hitPoints = 1;
    [SerializeField] public int lifes = 5;
    private bool invincible = false;
    private float invincibleTimer = 0;
    private float invincibleTime = 4;
    #endregion

    #region shooting
    [SerializeField] private BarController heatBar;
    [SerializeField] private float holdFireInterval = 1f;
    [SerializeField] private float holdFireTimer = 0f;
    [SerializeField] private float holdFireTime = 0f;
    private float holdFireHeatLimit = 6.9f;
    private bool holdFireIsOnCooldown = false;
    private float holdFireCooldownTimer = 0f;
    private float holdFireCooldownLimit = 2f;
    public bool holdFireCooling = false;

    [SerializeField] private Image heatWarningSignTop;
    [SerializeField] private Image heatWarningSignBottom;

    private float warningTimer;
    private float warningInterval = 0.5f;
    private bool warningActive = false;
    #endregion

    #region power ups
    private PowerUpTimer powerUpTimer;

    private FallingStar fallingStar;
    private BoundaryCheck boundaryCheck;
    public bool starUsed = false;

    #region shield
    private GameObject shield;
    private bool isShieldEnabled = false;
    private float shieldTimer = 4;
    private float shieldTimerReset = 4;
    private float shieldTimeLimit = 0;
    private int shieldLives = 3;
    private int shieldLivesReset = 3;
    #endregion

    #region machineGun
    [SerializeField] private bool machineGunEnabled = false;
    [SerializeField] private float machineGunFireInterval = 0.1f;
    [SerializeField] private float machineGunFireTimer = 0.1f;
    private float machineGunTimer = 5;
    private float machineGunTimerLimit = 0;
    private float machineGunTimerReset = 5;
    #endregion

    #region bigbullet
    [SerializeField] private bool bigBulletEnabled = false;
    private float bigBulletFireInterval = 0.2f;
    [SerializeField] private float bigBulletFireTimer = 0.1f;
    private float bigBulletTimer = 2;
    private float bigBulletTimerLimit = 0;
    private float bigBulletTimerReset = 2;
    #endregion

    #region more speed
    private float moreSpeedTimer = 3;
    private float moreSpeedTimerReset = 3;
    private float moreSpeedTimerLimit = 0;
    private bool moreSpeedEnabled = false;
    #endregion

    #endregion

    #region exhausts
    [SerializeField] private ParticleSystem BackExhaustParticles;
    [SerializeField] private ParticleSystem TopExhaustParticles;
    [SerializeField] private ParticleSystem BottomExhaustParticles;
    [SerializeField] private ParticleSystem FrontEx1Particles;
    [SerializeField] private ParticleSystem FrontEx2Particles;
    #endregion

    private void Start()
    {
        speedUpDebug = FindObjectOfType<SpeedUpDebug>();
        anim = GetComponent<Animator>();
        fallingStar = FindObjectOfType<FallingStar>();
        boundaryCheck = FindObjectOfType<BoundaryCheck>();
        powerUpTimer = FindObjectOfType<PowerUpTimer>();
        shield = GameObject.Find("Shield");
        shield.SetActive(false);
        rb = GetComponent<Rigidbody>();
        guns = GetComponentsInChildren<Gun>();
        BackExhaustParticles.Stop();
        TopExhaustParticles.Stop();
        BottomExhaustParticles.Stop();
        FrontEx1Particles.Stop();
        FrontEx2Particles.Stop();

        healthBar.SetValue(lifes);
    }

    private void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        #region cant leave screen
        if (transform.position.x <= 0 && inputX == -1)
        {
            inputX = 0;
        }
        else if (transform.position.x >= 18 && inputX == 1)
        {
            inputX = 0;
        }
        if (transform.position.y <= 0 && inputY == -1)
        {
            inputY = 0;
        }
        else if (transform.position.y >= 10 && inputY == 1)
        {
            inputY = 0;
        }

        #region animation rotation

        if (inputY < 0){
            anim.SetBool("GoDown", true);
            anim.SetBool("GoUp", false);
        } else if (inputY > 0) {
            anim.SetBool("GoDown", false);
            anim.SetBool("GoUp", true);
        } else if (inputY == 0) {
            anim.SetBool("GoDown", false);
            anim.SetBool("GoUp", false);
        }

        #endregion

        rb.velocity = new Vector2(inputX * moveSpeed, inputY * moveSpeed);

        #region Shooting
        if (Input.GetKeyDown(KeyCode.Space) && !speedUpDebug.paused && !holdFireIsOnCooldown && !machineGunEnabled && !bigBulletEnabled)
        {
            holdFireCooling = false;
            foreach (Gun gun in guns)
            {
                gun.Shoot();
            }
        }

        if (Input.GetKey(KeyCode.Space) && !speedUpDebug.paused && !holdFireIsOnCooldown && !machineGunEnabled && !bigBulletEnabled)
        {
            holdFireTimer += Time.deltaTime;
            holdFireTime += Time.deltaTime;
            if (holdFireTime > holdFireHeatLimit)
            {
                holdFireIsOnCooldown = true;
            }
            if (holdFireTimer > holdFireInterval)
            {
                foreach (Gun gun in guns)
                {
                    gun.Shoot();
                }
                holdFireTimer = 0;
            }
        }

        if (holdFireIsOnCooldown)
        {
            holdFireCooldownTimer += Time.deltaTime;
            if (holdFireCooldownTimer > holdFireCooldownLimit)
            {
                holdFireCooling = true;
                holdFireCooldownTimer = 0;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space) && !speedUpDebug.paused && !holdFireIsOnCooldown)
        {
            holdFireTimer = 0;
            holdFireCooling = true;
        }

        if (holdFireCooling)
        {
            if (holdFireTime > 0 && !holdFireIsOnCooldown)
            {
                holdFireTime -= Time.deltaTime / 1.2f;
            }
            else if (holdFireTime > 0 && holdFireIsOnCooldown)
            {
                holdFireTime -= Time.deltaTime * 1.7f;
            }
            if (holdFireIsOnCooldown && holdFireTime <= 0)
            {
                holdFireCooling = false;
                holdFireIsOnCooldown = false;
            }
        }

        heatBar.SetFloatValue(holdFireTime);

        if (holdFireTime > 4f || holdFireIsOnCooldown)
        {
            warningActive = true;
        }

        if (warningActive)
        {
            warningTimer += Time.deltaTime;
            if (warningTimer > warningInterval)
            {
                if (heatWarningSignTop.enabled)
                {
                    heatWarningSignTop.enabled = false;
                    heatWarningSignBottom.enabled = false;
                }
                else if (!heatWarningSignTop.enabled)
                {
                    heatWarningSignTop.enabled = true;
                    heatWarningSignBottom.enabled = true;
                }
                warningTimer = 0;
            }
            if (!holdFireIsOnCooldown && holdFireTime < 4f)
            {
                warningTimer = 0;
                heatWarningSignTop.enabled = false;
                heatWarningSignBottom.enabled = false;
                warningActive = false;
            }
            else if (holdFireIsOnCooldown && holdFireTime <= 0)
            {
                warningTimer = 0;
                heatWarningSignTop.enabled = false;
                heatWarningSignBottom.enabled = false;
                warningActive = false;
            }
        }

        #region powerup
        if (Input.GetKeyDown(KeyCode.F) && boundaryCheck.InsideOfBoundsCheck() && !starUsed)
        {
            if (!machineGunEnabled && !powerUpTimer.powerUpOnCooldown[1])
            {
                UsePowerUp();
                powerUpTimer.UseMachineGun();
                holdFireCooling = true;
                machineGunEnabled = true;
            }
        }
        if (machineGunEnabled)
        {
            machineGunTimer -= Time.deltaTime;
            if (machineGunTimer <= machineGunTimerLimit)
            {
                machineGunTimer = 0;
                if (Input.GetKey(KeyCode.Space))
                {
                    holdFireCooling = false;
                }
                machineGunEnabled = false;
                machineGunTimer = machineGunTimerReset;
            }
            machineGunFireTimer += Time.deltaTime;
            if (machineGunFireTimer >= machineGunFireInterval)
            {
                machineGunFireTimer = 0;
                foreach (Gun gun in guns)
                {
                    gun.Shoot();
                }
            }
        }

            if (Input.GetKeyDown(KeyCode.Q) && boundaryCheck.InsideOfBoundsCheck() && !starUsed)
        {
            if (!bigBulletEnabled && !powerUpTimer.powerUpOnCooldown[2])
            {
                UsePowerUp();
                powerUpTimer.UseBigBullet();
                bigBulletEnabled = true;
            }
        }

        if (bigBulletEnabled)
        {
            bigBulletTimer -= Time.deltaTime;
            if (bigBulletTimer <= bigBulletTimerLimit)
            {
                bigBulletTimer = 0;
                bigBulletEnabled = false;
                bigBulletTimer = bigBulletTimerReset;
            }
            bigBulletFireTimer += Time.deltaTime;
            if (bigBulletFireTimer >= bigBulletFireInterval)
            {
                bigBulletFireTimer = 0;
                foreach (Gun gun in guns)
                {
                    gun.ShootBig();
                }
            }
        }
        #endregion
        #endregion

        #region Shield
        if (isShieldEnabled)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= shieldTimeLimit)
            {
                shield.SetActive(false);
                isShieldEnabled = false;
                shieldLives = shieldLivesReset;
                shieldTimer = shieldTimerReset;
            }
        }
        if (Input.GetKeyDown(KeyCode.E) && boundaryCheck.InsideOfBoundsCheck() && !starUsed)
        {
            if (!isShieldEnabled && !powerUpTimer.powerUpOnCooldown[0])
            {
                UsePowerUp();
                powerUpTimer.UseShield();
                shield.SetActive(true);
                isShieldEnabled = true;
            }
        }
        #endregion

        #region more speed
        if (moreSpeedEnabled)
        {
            if (moreSpeedTimer > moreSpeedTimerLimit)
            {
                moreSpeedTimer -= Time.deltaTime;
            }
            if (moreSpeedTimer <= moreSpeedTimerLimit)
            {
                moreSpeedEnabled = false;
                moreSpeedTimer = moreSpeedTimerReset;
                moveSpeed /= 2;
            }
        }
        #endregion

        #region respawn
        if (invincible)
        {
            invincibleTimer += Time.deltaTime;
            if (invincibleTimer >= invincibleTime)
            {
                invincible = false;
                invincibleTimer = 0;
                shield.SetActive(false);
                isShieldEnabled = false;
                shieldLives = shieldLivesReset;
                shieldTimer = shieldTimerReset;
            }
        }
        #endregion

        #region Exhaust particles
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            BackExhaustParticles.Play();
        }
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            BackExhaustParticles.Stop();
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            BottomExhaustParticles.Play();
        }
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            BottomExhaustParticles.Stop();
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            TopExhaustParticles.Play();
        }
        if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            TopExhaustParticles.Stop();
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            FrontEx1Particles.Play();
        }
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            FrontEx1Particles.Stop();
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            FrontEx2Particles.Play();
        }
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            FrontEx2Particles.Stop();
        }
        #endregion
    }

    private void OnTriggerEnter(Collider collision)
    {
        #region ExplodeAsteroid

        if (collision.gameObject.CompareTag("Asteroid")){
            collision.GetComponent<Asteroid>().Explode();
        }

        #endregion

        #region take damage
        if (collision.gameObject.CompareTag("EnemyBullet") || collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Asteroid"))
        {
            if (!isShieldEnabled && !invincible)
            {
                powerUpTimer.ResetShieldTimer();
                hitPoints--;
            }

            if (hitPoints <= 0 && !invincible)
            {
                lifes--;
                healthBar.SetValue(lifes);
                invincible = true;
                hitPoints = 1;
                if (!isShieldEnabled)
                {
                    powerUpTimer.UseShield();
                    shield.SetActive(true);
                    isShieldEnabled = true;
                }
            }
            if (lifes <= 0)
            {
                powerUpTimer.ResetShieldTimer();
                this.gameObject.SetActive(false);
                LevelController.instance.RestartGame();
            }
        }
        #endregion

        #region collect powerUp
        if (collision.gameObject.CompareTag("PowerUp"))
        {
            PowerUp powerUp = collision.GetComponent<PowerUp>();

            #region Shield
            if (powerUp.shield)
            {
                if (!isShieldEnabled)
                {
                    shield.SetActive(true);
                    isShieldEnabled = true;
                }
                Destroy(powerUp.gameObject);
            }
            #endregion

            #region quickFire
            if (powerUp.machineGun)
            {
                if (!machineGunEnabled)
                {
                    machineGunEnabled = true;
                }
                Destroy(powerUp.gameObject);
            }
            #endregion

            #region big bullet
            if (powerUp.bigBullet)
            {
                foreach (Gun gun in guns)
                {
                    gun.ShootBig();
                }
                Destroy(powerUp.gameObject);
            }
            #endregion

            #region more speed
            if (powerUp.moreSpeed)
            {
                moveSpeed *= 2;
                moreSpeedEnabled = true;
                Destroy(powerUp.gameObject);
            }
            #endregion
        }
        #endregion
    }

    private void UsePowerUp()
    {
        starUsed = true;
        fallingStar.ExplodeStar();
    }

    public void RegenHealth()
    {
        lifes = 5;
        healthBar.SetValue(lifes);
        BackExhaustParticles.Stop();
        TopExhaustParticles.Stop();
        BottomExhaustParticles.Stop();
        FrontEx1Particles.Stop();
        FrontEx2Particles.Stop();
    }

    public void RegenOneHealth()
    {
        if (lifes < 5)
        {
            lifes++;
        }
        healthBar.SetValue(lifes);
    }

    public void RegenFullHealth()
    {
        if (lifes < 5)
        {
            lifes = 5;
        }
        healthBar.SetValue(lifes);
    }
}
