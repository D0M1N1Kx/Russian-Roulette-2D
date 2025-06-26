using Godot;
using System;
using System.Threading.Tasks;

public partial class RussianRoulette : Node2D
{
	//UI elemek
	[Export]
	private AnimatedSprite2D player;
	[Export]
	private AnimatedSprite2D AI;
	private Label gameStatus;
	private Button shootSelf;
	private Button shootEnemy;
	private RichTextLabel GameRules;
	//Logikai elemek
	private int[] chamber = new int[6];
	private int chamberIndex = 0;
	private bool isGameActive = false;
	private bool PlayerTurn = false;
	private bool EnemyTurn = false;
	Random random = new Random();
	bool restartChoice = false;
	//inditáskor lefut 1 alkalommal
	public override void _Ready()
	{
		//editor elemeinek betöltése változókba
		player = GetNode<AnimatedSprite2D>("Player");
		AI = GetNode<AnimatedSprite2D>("AI");
		gameStatus = GetNode<Label>("GameStatus");
		shootSelf = GetNode<Button>("ShootSelf");
		shootEnemy = GetNode<Button>("ShootEnemy");
		GameRules = GetNode<RichTextLabel>("GameRules");
		//alapból az Idle animációt játsza le mindkét playernél
		PlayAnimation(player, "Idle", false);
		PlayAnimation(AI, "Idle", true);
		//gameStatus szövegét semmire állitsuk, illetve eltüntessük alapból a gombokat
		shootSelf.Visible = false;
		shootEnemy.Visible = false;
		gameStatus.Text = "";
		GameRules.Visible = true;
		//elinditsuk a játékot
		GetTree().CreateTimer(10.0f).Timeout += StartGame;
	}
	private void StartGame()
	{
		//játékot elinditsuk, chamberIndexet nullázzuk, újratöltsük a revolvert, illetve kiválasszuk ki kezd
		isGameActive = true;
		GameRules.Visible = false;
		chamberIndex = 0;
		ReloadChamber();
		if (random.Next(0, 2) == 0)
		{
			PlayerTurn = true;
			EnemyTurn = false;
			GD.Print("Player Turn");
			gameStatus.AddThemeColorOverride("font_color", new Color(229, 229, 0));
			gameStatus.Text = "Your Turn!";
			ShowPlayerButtons();
		}
		else
		{
			EnemyTurn = true;
			PlayerTurn = false;
			GD.Print("Enemy Turn");
			gameStatus.AddThemeColorOverride("font_color", new Color(255, 0, 0));
			gameStatus.Text = "Enemy Turn!";
			HidePlayerButtons();
			GetTree().CreateTimer(3.0f).Timeout += AIMakeChoice;
		}
	}
	private void ReloadChamber()
	{
		//0(vak töltény) 1(éles), először 6 blanket töltünk bele és utánna ez egyikbe random egy éleset
		for (int i = 0; i < chamber.Length; i++)
		{
			chamber[i] = 0;
		}
		int number = random.Next(0, 6);
		chamber[number] = 1;
		GD.Print($"A {number + 1}. töltény éles.");
	}
	public void PlayAnimation(AnimatedSprite2D sprite, string animationName, bool flipH)
	{
		sprite.FlipH = flipH;
		sprite.Play(animationName);
	}
	private int CheckChamberCurrentRound()
	{
		GD.Print($"A/Az {chamberIndex+1}. töltény: {chamber[chamberIndex]}");

		int result = chamber[chamberIndex];
		chamberIndex++;
		if (chamberIndex >= 6)
		{
			GD.Print("Kamra üres, újratöltés ...");
			chamberIndex = 0;
			ReloadChamber();
		}
		return result;
	}
	private void ShowPlayerButtons()
	{
		if (PlayerTurn)
		{
			shootEnemy.Visible = true;
			shootSelf.Visible = true;
		}
	}
	private void HidePlayerButtons()
	{
		if (EnemyTurn)
		{
			shootEnemy.Visible = false;
			shootSelf.Visible = false;
		}
	}
	private void _on_shoot_self_button_down()
	{
		if (isGameActive && PlayerTurn)
		{
			ShootSelf();
		}
		//gomb lenyomásakor ha a restart a kérdés akkor kilép
		if (restartChoice == true)
		{
			GetTree().Quit();
		}
	}
	private void _on_shoot_enemy_button_down()
	{
		if (isGameActive && PlayerTurn)
		{
			ShootEnemy();
		}
		//gomb lenyomásakor ha a restart a kérdés újrainditsa a játékot
		if (restartChoice == true)
		{
			gameStatus.AddThemeColorOverride("font_color", new Color(229, 229, 0));
			gameStatus.Text = "Restarting ...";
			shootSelf.Visible = false;
			shootEnemy.Visible = false;
			GetTree().CreateTimer(5.0f).Timeout += RestartGame;
			shootSelf.Text = "Shoot Self";
			shootEnemy.Text = "Shoot Enemy";
			restartChoice = false;
		}
	}
	private void ShootSelf()
	{
		if (isGameActive)
		{
			HidePlayerButtons();
			int roundResult = CheckChamberCurrentRound();
			if (roundResult == 1)
			{
				if (PlayerTurn)
				{
					PlayAnimation(player, "ShootHimselfLive", false);
					GetTree().CreateTimer(2.0f).Timeout += () =>
					{
						PlayAnimation(player, "Dead", false);
						gameStatus.AddThemeColorOverride("font_color", new Color(229, 229, 0));
						gameStatus.Text = "You're Dead! Because You Shoot Yourself With a Live Round!";
						EndGame(false);
					};
				}
				else if (EnemyTurn)
				{
					PlayAnimation(AI, "ShootHimselfLive", true);
					GetTree().CreateTimer(2.0f).Timeout += () =>
					{
						PlayAnimation(AI, "Dead", true);
						gameStatus.AddThemeColorOverride("font_color", new Color(255, 0, 0));
						gameStatus.Text = "Enemy Shot Himself With a Live Round! You Win!";
						EndGame(true);
					};
				}
			}
			else if (roundResult == 0)
			{
				if (PlayerTurn)
				{
					PlayAnimation(player, "ShootHimselfBlank", false);
					GetTree().CreateTimer(2.0f).Timeout += () =>
					{
						PlayAnimation(player, "Idle", false);
						gameStatus.AddThemeColorOverride("font_color", new Color(229, 229, 0));
						gameStatus.Text = "Blank Round! You Get Another Turn!";
						GetTree().CreateTimer(2.0f).Timeout += ShowPlayerButtons;
					};
				}
				else if (EnemyTurn)
				{
					PlayAnimation(AI, "ShootHimselfBlank", true);
					GetTree().CreateTimer(2.0f).Timeout += () =>
					{
						PlayAnimation(AI, "Idle", true);
						gameStatus.AddThemeColorOverride("font_color", new Color(255, 0, 0));
						gameStatus.Text = "Enemy Got a Blank! Enemy Gets Another Turn!";
						GetTree().CreateTimer(2.0f).Timeout += AIMakeChoice;
					};
				}
			}
		}
		else
		{
			return;
		}
	}
	private void ShootEnemy()
	{
		if (isGameActive)
		{
			HidePlayerButtons();
			int roundResult = CheckChamberCurrentRound();
			if (roundResult == 1)
			{
				if (PlayerTurn)
				{
					PlayAnimation(player, "ShootLive", false);
					PlayAnimation(AI, "HurtDead", true);
					GetTree().CreateTimer(1.5f).Timeout += () =>
					{
						PlayAnimation(AI, "Dead", true);
						PlayAnimation(player, "Idle", false);
						gameStatus.AddThemeColorOverride("font_color", new Color(229, 229, 0));
						gameStatus.Text = "You Shot Your Opponent With a Live Round! You Win";
						EndGame(true);
					};
				}
				else if (EnemyTurn)
				{
					PlayAnimation(AI, "ShootLive", true);
					PlayAnimation(player, "HurtDead", false);
					GetTree().CreateTimer(1.5f).Timeout += () =>
					{
						PlayAnimation(player, "Dead", false);
						PlayAnimation(AI, "Idle", true);
						gameStatus.AddThemeColorOverride("font_color", new Color(255, 0, 0));
						gameStatus.Text = "Enemy Shot You With a Live Round! You Lose!";
						EndGame(false);
					};
				}
			}
			else if (roundResult == 0)
			{
				if (PlayerTurn)
				{
					PlayAnimation(player, "ShootBlank", false);
					GetTree().CreateTimer(2.0f).Timeout += () =>
					{
						PlayAnimation(player, "Idle", false);
						gameStatus.AddThemeColorOverride("font_color", new Color(229, 229, 0));
						gameStatus.Text = "Blank Round! Now It's Enemy's Turn!";
						SwitchToEnemyTurn();
					};
				}
				else if (EnemyTurn)
				{
					PlayAnimation(AI, "ShootBlank", true);
					GetTree().CreateTimer(2.0f).Timeout += () =>
					{
						PlayAnimation(AI, "Idle", true);
						gameStatus.AddThemeColorOverride("font_color", new Color(255, 0, 0));
						gameStatus.Text = "Enemy Shot a Blank! Now It's Your Turn!";
						SwitchToPlayerTurn();
					};
				}
			}
		}
	}
	private void SwitchToPlayerTurn()
	{
		PlayerTurn = true;
		EnemyTurn = false;
		GetTree().CreateTimer(2.0f).Timeout += ShowPlayerButtons;
	}
	private void SwitchToEnemyTurn()
	{
		PlayerTurn = false;
		EnemyTurn = true;
		GetTree().CreateTimer(2.0f).Timeout += AIMakeChoice;
	}
	private void AIMakeChoice()
	{
		if (isGameActive)
		{
			gameStatus.Text = "Enemy is thinking ... ";
			int ChoiceNumber = random.Next(0, 2);
			if (ChoiceNumber == 0)
			{
				ShootSelf();
				GD.Print("AI lelövi magát");
			}
			else
			{
				ShootEnemy();
				GD.Print("AI a playert lövi le");
			}
		}
		else
		{
			return;
		}
	}
	private void EndGame(bool playerWon)
	{
		isGameActive = false;
		HidePlayerButtons();
		if (playerWon)
		{
			PlayAnimation(AI, "Dead", true);
			PlayAnimation(player, "Idle", false);
		}
		else
		{
			PlayAnimation(player, "Dead", false);
			PlayAnimation(AI, "Idle", true);
		}
		shootSelf.Visible = true;
		shootEnemy.Visible = true;
		gameStatus.Text = "Are You Want To Restart?";
		shootSelf.Text = "No";
		shootSelf.AddThemeColorOverride("font_color", new Color(255, 0, 0));
		shootEnemy.Text = "Yes";
		shootEnemy.AddThemeColorOverride("font_color", new Color(229, 229, 0));
		restartChoice = true;
	}
	private void RestartGame()
	{
		PlayAnimation(player, "Idle", false);
		PlayAnimation(AI, "Idle", true);
		StartGame();
	}
}
