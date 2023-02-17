using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject menuCam;
    public GameObject gameCam;
    public Player player;
    public BossEnemy boss;

    //전투 시작시 비활성화
    public GameObject itemShop;
    public GameObject weaponShop;
    public GameObject startZone;

    public int stage;
    public float playTime;
    public bool isBattle;

    public int enemyStateA;
    public int enemyStateB;
    public int enemyStateC;
    public int enemyStateBoss;

    public Transform[] enemySpawnZone;
    public GameObject[] enemies;
    public List<int> enemyList;

    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;
    public TextMeshProUGUI hiScoreText;

    //상태 텍스트
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI playTimeText;
    public TextMeshProUGUI playerCoinText;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI playerAmmoText;
    public TextMeshProUGUI playerGrenadeText;
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI bestScoreText;

    //상태 이미지
    public Image weapon1;
    public Image weapon1a;
    public Image weapon2;
    public Image weapon3;

    //Boss HP 이미지
    public RectTransform bossHP;
    public RectTransform bossMaxHealth; //전체 HP
    public RectTransform bossCurHealth; //현재 HP


    private void Awake()
    {
        enemyList = new List<int>();
        hiScoreText.text = string.Format("{0:n0}", PlayerPrefs.GetInt("HIScore"));
    }

    public void GameStart()
    {
        //게임 실행시 Player 활성화, 시작메뉴 비활성화, 상태메뉴 활성화
        player.gameObject.SetActive(true);

        menuCam.SetActive(false);
        gameCam.SetActive(true);

        menuPanel.SetActive(false);
        gamePanel.SetActive(true);
    }

    public void GameOver()
    {
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(true);
        currentScoreText.text = scoreText.text;

        int maxScore = PlayerPrefs.GetInt("HIScore");

        if(player.score > maxScore)
        {
            bestScoreText.gameObject.SetActive(true);
            PlayerPrefs.SetInt("HIScore", player.score);
        }
    }

    public void MainTitle()
    {
        SceneManager.LoadScene(0);
    }

    public void StageStart()
    {
        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        startZone.SetActive(false);

        foreach(Transform zone in enemySpawnZone)
        {
            zone.gameObject.SetActive(true);
        }

        isBattle = true;
        StartCoroutine(IsBattle());
    }

    public void StageEnd()
    {
        player.transform.position = Vector3.left * 1.0f;

        itemShop.SetActive(true);
        weaponShop.SetActive(true);
        startZone.SetActive(true);

        foreach (Transform zone in enemySpawnZone)
        {
            zone.gameObject.SetActive(false);
        }

        isBattle = false;
        stage++;
    }

    private IEnumerator IsBattle()
    {
        if(stage % 5 == 0)
        {
            enemyStateBoss++;

            GameObject instantEnemy = Instantiate(enemies[3], enemySpawnZone[4].position, enemySpawnZone[4].rotation);
            Enemy enemy = instantEnemy.GetComponent<Enemy>();
            enemy.target = player.gameObject;
            enemy.gameManager = this;
            boss = instantEnemy.GetComponent<BossEnemy>();
        }
        else
        {
            for (int index = 0; index < stage; index++)
            {
                int ran = Random.Range(0, 3);
                enemyList.Add(ran);

                switch (ran)
                {
                    case 0:
                        enemyStateA++;
                        break;
                    case 1:
                        enemyStateB++;
                        break;
                    case 2:
                        enemyStateC++;
                        break;
                }
            }

            while (enemyList.Count > 0)
            {
                int ranZone = Random.Range(0, 4);
                GameObject instantEnemy = Instantiate(enemies[enemyList[0]], enemySpawnZone[ranZone].position, enemySpawnZone[ranZone].rotation);
                Enemy enemy = instantEnemy.GetComponent<Enemy>();
                enemy.target = player.gameObject;
                enemy.gameManager = this;

                //생성후 데이터 삭제
                enemyList.RemoveAt(0);
                yield return new WaitForSeconds(4.0f);
            }
        }

        while (enemyStateA + enemyStateB + enemyStateC + enemyStateBoss > 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(3.0f);
        boss = null;

        StageEnd();
    }

    private void Update()
    {
        if(isBattle)
        {
            playTime += Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        //상단 UI
        scoreText.text = string.Format("{0:n0}", player.score);
        stageText.text = "STAGE " + stage;

        int hour = (int)(playTime / 3600);
        int min = (int)((playTime - hour * 3600) / 60);
        int second = (int)(playTime % 60);

        playTimeText.text = string.Format("{0:00}", hour) + ":" + string.Format("{0:00}", min) + ":" + string.Format("{0:00}", second);

        //하단 UI
        playerCoinText.text = string.Format("{0:n0}", player.coin);
        playerHealthText.text = player.health + " / " + player.maxHealth;
        playerGrenadeText.text = player.hasgrenade + " / " + player.maxHasGrenade;

        if(player.currentWeapon == null || player.currentWeapon.type == Weapon.Type.Melee)
        {
            playerAmmoText.text = "- / " + player.ammo;
        }
        else
        {
            playerAmmoText.text = player.currentWeapon.curAmmo + " / " + player.ammo;
        }

        //weapon UI
        weapon1.color = new Color(0, 0, 0, player.hasWeapons[0] ? 1 : 0);
        weapon1a.color = new Color(0, 0, 0, player.hasWeapons[0] ? 1 : 0);
        weapon2.color = new Color(0, 0, 0, player.hasWeapons[1] ? 1 : 0);
        weapon3.color = new Color(0, 0, 0, player.hasWeapons[2] ? 1 : 0);

        //Boss Health
        if(boss != null)
        {
            bossHP.anchoredPosition = Vector3.down * 50;
            bossCurHealth.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1);
        }
        else
        {
            bossHP.anchoredPosition = Vector3.up * 200;
        }
    }
}
