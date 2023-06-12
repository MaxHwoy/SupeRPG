using SupeRPG.Game;
using SupeRPG.Input;
using SupeRPG.UI;

using System;
using System.Collections;

using Unity.VisualScripting;

using UnityEngine;

using Object = UnityEngine.Object;

namespace SupeRPG.Battle
{
    public class BattleManager : MonoBehaviour
    {
        public enum BattleState
        {
            None,
            StartBattle,
            PlayerMove,
            EnemyMove,
            FinishBattle,
        }

        public enum BattleOutcome
        {
            Exit,
            Victory,
            Defeat,
        }

        private static readonly Vector2[][] ms_enemyPositions = new Vector2[][]
        {
            new Vector2[0]
            {
            },
            new Vector2[1]
            {
                new Vector2(+0.6600f, +0.1600f),
            },
            new Vector2[2]
            {
                new Vector2(+0.3000f, +0.5000f),
                new Vector2(+0.7125f, +0.0000f),
            },
            new Vector2[3]
            {
                new Vector2(+0.6450f, +0.5400f),
                new Vector2(+0.7200f, +0.0000f),
                new Vector2(+0.2700f, +0.2600f),
            },
            new Vector2[4]
            {
                new Vector2(+0.3750f, +0.5400f),
                new Vector2(+0.7800f, +0.5400f),
                new Vector2(+0.7800f, +0.0000f),
                new Vector2(+0.3750f, +0.0000f),
            },
        };

        private static readonly Vector2 ms_playerPosition = new(-0.6600f, +0.1600f);

        private static BattleManager ms_instance;

        private SpriteRenderer m_background;
        private GameObject m_parentObj;
        private Action m_onBattleEnded;
        private float m_backgroundRatio;

        private BattleBehavior[] m_enemyBehaviors;
        private BattleBehavior m_playerBehavior;

        private Enemy[] m_enemyEntities;
        private Player m_playerEntity;

        private BattleOutcome m_outcome;
        private BattleState m_state;
        private int m_abilityIndex;

        private int m_victoryReward;
        private int m_defeatReward;

        private Coroutine m_currentRoutine;
        private bool m_forceExit;

        [SerializeField]
        private BattleBuilder BattleUI;

        [SerializeField]
        private GameObject EntityPrefab;

        public static BattleManager Instance => ms_instance == null ? (ms_instance = FindFirstObjectByType<BattleManager>()) : ms_instance;

        public BattleOutcome Outcome => this.m_state == BattleState.FinishBattle ? this.m_outcome : throw new Exception("Cannot decide battle outcome when it is not finished");

        public void StartBattle(Player player, Enemy[] enemies, int victoryReward, int defeatReward, Action onBattleStarted, Action onBattleEnded)
        {
            if (this.m_state != BattleState.None)
            {
                throw new Exception("We are already in battle!");
            }

            if (this.BattleUI == null)
            {
                throw new Exception("Cannot start battle because Battle UI is null!");
            }

            if (player is null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (enemies is null)
            {
                throw new ArgumentNullException(nameof(enemies));
            }

            this.m_state = BattleState.StartBattle;
            this.m_outcome = BattleOutcome.Exit;

            this.m_victoryReward = victoryReward;
            this.m_defeatReward = defeatReward;
            this.m_forceExit = false;
            this.m_onBattleEnded = onBattleEnded;
            this.m_playerEntity = player;
            this.m_enemyEntities = enemies;
            this.m_abilityIndex = -2;

            this.SetupCallbacks(true);

            this.BattleUI.LockActions();

            this.m_currentRoutine = this.StartCoroutine(this.PerformBattleStart(onBattleStarted));
        }

        public void FinishBattle()
        {
            this.BattleUI.LockActions();
            this.BattleUI.CurrentEntity = null;

            if (this.m_currentRoutine != null)
            {
                this.StopCoroutine(this.m_currentRoutine);
            }

            this.MaybeChangeCursorTexture(true);
            this.SetupCallbacks(false);

            Object.Destroy(this.m_parentObj);

            this.m_playerEntity?.FinishBattle();

            if (this.m_enemyEntities is not null)
            {
                for (int i = 0; i < this.m_enemyEntities.Length; ++i)
                {
                    this.m_enemyEntities[i]?.FinishBattle();
                }
            }

            this.m_background = null;
            this.m_playerEntity = null;
            this.m_enemyEntities = null;
            this.m_playerBehavior = null;
            this.m_enemyBehaviors = null;
            this.m_forceExit = false;
            this.m_abilityIndex = -2;
            this.m_onBattleEnded = null;
            this.m_currentRoutine = null;
            this.m_state = BattleState.None;
            this.m_outcome = BattleOutcome.Exit;
        }

        private void InitializePlayer()
        {
            var behavior = GameObject.Instantiate(this.EntityPrefab).GetComponent<BattleBehavior>();

            behavior.gameObject.transform.parent = this.m_parentObj.transform;
            behavior.UnitOrigin = ms_playerPosition;
            behavior.DefaultScale = 1.25f;
            behavior.MaximumScale = 1.25f;
            behavior.MinimumScale = 1.10f;
            behavior.AnimationSpeed = 0.2f;
            behavior.NoneIsIdleAnimation = true;
            behavior.Index = 0;

            behavior.Initialize(this.m_playerEntity, "Player Object", 7); // player sprites are slightly bigger

            this.m_playerBehavior = behavior;

            this.m_playerEntity.InitBattle();
        }

        private void InitializeEnemies()
        {
            var enemyPositions = ms_enemyPositions[this.m_enemyEntities.Length];

            this.m_enemyBehaviors = new BattleBehavior[this.m_enemyEntities.Length];

            for (int i = 0; i < this.m_enemyBehaviors.Length; ++i)
            {
                var behavior = GameObject.Instantiate(this.EntityPrefab).GetComponent<BattleBehavior>();

                behavior.gameObject.transform.parent = this.m_parentObj.transform;
                behavior.UnitOrigin = enemyPositions[i];
                behavior.DefaultScale = 1.0f;
                behavior.MaximumScale = 1.0f;
                behavior.MinimumScale = 0.9f;
                behavior.AnimationSpeed = 0.12f;
                behavior.NoneIsIdleAnimation = true;
                behavior.Index = i;

                behavior.Initialize(this.m_enemyEntities[i], "Enemy Object " + i.ToString(), 5);

                this.m_enemyBehaviors[i] = behavior;
                this.m_enemyEntities[i].InitBattle();
            }
        }

        private void InitializeBackground()
        {
            var scaleY = ScreenManager.Instance.OrthographicSize * 0.4f; // (orthoSize / 5) * 2

            this.m_background = new GameObject("Background").AddComponent<SpriteRenderer>();

            this.m_background.sprite = ResourceManager.LoadSprite("Sprites/Battle/BattleBackground");

            var size = this.m_background.bounds.size;

            this.m_backgroundRatio = size.y / size.x;

            this.m_background.gameObject.transform.parent = this.m_parentObj.transform;
            this.m_background.gameObject.transform.position = Vector3.zero;
            this.m_background.gameObject.transform.localScale = new Vector3(scaleY * ScreenManager.Instance.AspectRatio * this.m_backgroundRatio, scaleY, 1.0f);
        }

        private IEnumerator PerformBattleStart(Action callback)
        {
            this.m_state = BattleState.StartBattle;

            this.m_parentObj = new GameObject("Battle");

            yield return null;

            this.InitializeBackground();

            yield return null;

            this.InitializePlayer();

            yield return null;

            this.InitializeEnemies();

            yield return null;

            this.BattleUI.AddHealthIndicator(this.m_playerBehavior.GetPositionForHealthIndicator, this.m_playerEntity);

            yield return null;

            for (int i = 0; i < this.m_enemyBehaviors.Length; ++i)
            {
                this.BattleUI.AddHealthIndicator(this.m_enemyBehaviors[i].GetPositionForHealthIndicator, this.m_enemyEntities[i]);
            }

            this.m_playerBehavior.PlayAnimation(BattleBehavior.AnimationType.Idle);

            yield return null;

            for (int i = 0; i < this.m_enemyBehaviors.Length; ++i)
            {
                this.m_enemyBehaviors[i].PlayAnimation(BattleBehavior.AnimationType.Idle);
            }

            yield return null;

            this.m_currentRoutine = this.StartCoroutine(this.PerformPlayerMove());

            callback?.Invoke();
        }

        private IEnumerator PerformBattleFinal(BattleOutcome outcome)
        {
            this.m_state = BattleState.FinishBattle;

            this.m_outcome = outcome;

            this.BattleUI.LockActions();

            yield return null;

            this.MaybeChangeCursorTexture(true);

            this.ForceUnhighlightAll();

            this.BattleUI.CurrentEntity = null;

            yield return null;

            if (outcome != BattleOutcome.Exit)
            {
                if (outcome == BattleOutcome.Victory)
                {
                    this.BattleUI.ShowGameOverOverlay("VICTORY", $"REWARD: ${this.m_victoryReward}", new Color32(255, 220, 0, 255), "Sprites/Battle/ResultVictory");
                }
                else
                {
                    this.BattleUI.ShowGameOverOverlay("DEFEAT", $"REWARD: ${this.m_defeatReward}", new Color32(255, 55, 55, 255), "Sprites/Battle/ResultDefeat");
                }

                yield return new WaitForSeconds(5.0f);
            }

            Debug.Log($"Finished the battle; outcome = {outcome}");

            this.m_onBattleEnded?.Invoke();
        }

        private IEnumerator PerformPlayerMove()
        {
            // change state to player move
            this.m_state = BattleState.PlayerMove;

            // check if force exit was requested
            if (this.m_forceExit)
            {
                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                yield break;
            }

            // log that player is moving now
            Debug.Log("Performing player move...");

            // update the turn label information to player one
            this.BattleUI.UpdateTurnLabel("YOUR MOVE", new Color32(0, 185, 0, 255));

            // reset selected ability index (-2 or less = no selection, -1 = basic attack, 0 or more = index of ability used)
            this.m_abilityIndex = -2;

            // change cursor texture based on where we are hovering
            this.MaybeChangeCursorTexture(false);

            // change the current entity displayed if we clicked on any ALIVE entity
            this.MaybeChangeEntityDisplayed();

            // wait a frame to submit all changes
            yield return null;

            // check if force exit was requested
            if (this.m_forceExit)
            {
                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                yield break;
            }

            // change cursor texture based on where we are hovering
            this.MaybeChangeCursorTexture(false);

            // change the current entity displayed if we clicked on any ALIVE entity
            this.MaybeChangeEntityDisplayed();

            // index of the enemy selected for attacking
            int attackedIndex;

            // regenerate stuff first
            this.m_playerEntity.Regenerate();

            // cooldown all player abilities and effects before player's turn starts (get total heal, mana, dmg received from effects)
            this.m_playerEntity.Cooldown(out int coolHeal, out int coolMana, out int coolDmgs);

            // if player entity is the currently displayed UI entity, update it
            if (this.m_playerEntity == this.BattleUI.CurrentEntity)
            {
                this.BattleUI.UpdateInterface();
            }

            // add total heal, mana and dmg received by player's effects to the screen as text animations
            this.TryAddAllyTextInformation(this.m_playerBehavior, null, coolHeal, coolMana, coolDmgs);

            // if player is no longer alive because of effects (such as poison, burn, etc.)
            if (!this.m_playerEntity.IsAlive) // this can happen b/c of poison and burn
            {
                // if player is the currently selected entity, no longer display it then
                if (this.BattleUI.CurrentEntity == this.m_playerEntity)
                {
                    this.BattleUI.CurrentEntity = null;
                }

                // remove health indicator for player since it is now dead
                this.BattleUI.RemoveHealthIndicator(this.m_playerEntity);

                // start playing death animation for player
                this.m_playerBehavior.PlayAnimation(BattleBehavior.AnimationType.Death);
            }
            else
            {
                // otherwise if player is currently displayed on UI, update it
                if (this.BattleUI.CurrentEntity == this.m_playerEntity)
                {
                    this.BattleUI.UpdateInterface();
                }

                // also, if player got any damage as a result of any effect, play damaged animation
                if (coolDmgs > 0)
                {
                    this.m_playerBehavior.PlayAnimation(BattleBehavior.AnimationType.Damaged);
                }
            }

            // wait until ALL animations finished playing (text animations + death animation if player died)
            while (true)
            {
                // in the meantime, if force exit was requested, exit
                if (this.m_forceExit)
                {
                    this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                    yield break;
                }

                // otherwise, change cursor texture based on where we are hovering
                this.MaybeChangeCursorTexture(false);

                // also change the current entity displayed if we clicked on any ALIVE entity
                this.MaybeChangeEntityDisplayed();

                // if all animations finished playing, break from the loop
                if (!this.HasAnyAnimationsPlaying())
                {
                    break;
                }

                // otherwise wait for next frame
                yield return null;
            }

            // if player is no longer alive and death animation finished playing, exit non-forcefully
            if (!this.m_playerEntity.IsAlive)
            {
                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Defeat));

                yield break;
            }

            // initialize all enemy turn stats for this current move
            for (int i = 0; i < this.m_enemyEntities.Length; ++i)
            {
                this.m_enemyEntities[i].InitTurn();
            }

            // wait for next frame so that everything updates
            yield return null;

            // otherwise, unlock actions for player on the UI
            this.BattleUI.UnlockActions();

            // wait until player selected a valid ability to use and a valid enemy to attack
            while (true)
            {
                // in the meantime, if force exit was requested, exit
                if (this.m_forceExit)
                {
                    this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                    yield break;
                }

                // if no ability was selected yet, allow cursor and entity displayed change per regular
                if (this.m_abilityIndex <= -2)
                {
                    // update cursor based on where we hover
                    this.MaybeChangeCursorTexture(false);

                    // update currently dispplayed entity on the UI
                    this.MaybeChangeEntityDisplayed();
                }
                else // otherwise, if an ability is now selected (callback from UI should be called)
                {
                    // initialize the attacked enemy index
                    attackedIndex = -1;
                    
                    // this is true if ability selected deals no damage or if a valid alive enemy was selected
                    var performAttack = false;

                    // if ability selected deals no damage, no need for player to select an enemy to attack
                    if (this.m_abilityIndex >= 0 && !this.m_playerEntity.Abilities[this.m_abilityIndex].DoesDamage)
                    {
                        performAttack = true;
                    }
                    else
                    {
                        // otherwise wait for player to select a valid alive enemy to attack

                        // we can change cursor texture to 'target' texture if we hover over an enemy
                        this.MaybeChangeCursorToTarget(false);

                        // we can also highlight enemy over which we are hovering, granted if it's a target enemy
                        this.MaybeHighlightTargetEnemies(false);

                        // note: do not allow changing currently selected entity for the UI until 'cancel' on the UI is clicked (callback)

                        // if we clicked this frame
                        if (InputProcessor.Instance.RaycastLeftClickIfHappened(out var collider))
                        {
                            // if we clicked a valid collider that is a valid alive enemy entity
                            if (collider != null && collider.gameObject.TryGetComponent<BattleBehavior>(out var behavior) && !behavior.Entity.IsPlayer && behavior.Entity.IsAlive)
                            {
                                // track this enemy for attack
                                attackedIndex = behavior.Index;

                                // perform the attack right afterwards
                                performAttack = true;
                            }
                        }
                    }

                    // if we can perform an attack with an ability now
                    if (performAttack)
                    {
                        // confirm to UI that actions are finished
                        this.BattleUI.ConfirmActionFinished();

                        // lock player from performing any other actions until next turn
                        this.BattleUI.LockActions();

                        // change cursor texture immediately from 'target' one
                        this.MaybeChangeCursorTexture(false);

                        // unhighlight targetted enemy (remove glowing effects)
                        this.ForceUnhighlightAll();

                        // break from this loop and go next
                        break;
                    }
                }

                // wait for next frame to happen
                yield return null;
            }

            // wait for next frame so that everything updates
            yield return null;

            // if forced exit was requested, quit now
            if (this.m_forceExit)
            {
                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                yield break;
            }

            // otherwise, if we selected an enemy to attack (doesn't happen ONLY if an ability deals NO damage)
            if (attackedIndex >= 0)
            {
                // start playing attack animation for player, targetting selected enemy
                this.m_playerBehavior.PlayAnimation(BattleBehavior.AnimationType.Attack, this.m_enemyBehaviors[attackedIndex]);

                // wait until attacking animation finishes playing
                while (true)
                {
                    // in the meantime, if forced exit was requested, quit now
                    if (this.m_forceExit)
                    {
                        this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                        yield break;
                    }

                    // otherwise update cursor texture to where we hover over
                    this.MaybeChangeCursorTexture(false);

                    // also allow now to change the entity displayed on the UI
                    this.MaybeChangeEntityDisplayed();

                    // if no more animations are playing, continue by exiting the loop
                    if (!this.HasAnyAnimationsPlaying())
                    {
                        break;
                    }

                    // otherwise wait for next frame to happen
                    yield return null;
                }
            }

            // wait for one more frame so that UI and everything else is updated
            yield return null;

            // if forced exit is requested, quit
            if (this.m_forceExit)
            {
                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                yield break;
            }

            // change cursor texture based on where we are hovering
            this.MaybeChangeCursorTexture(false);

            // change the current entity displayed if we clicked on any ALIVE entity
            this.MaybeChangeEntityDisplayed();

            // finally, perform an attack (player attacks selected enemy with selected ability) and get the outcome info
            var outcome = BattleUtility.Attack(this.m_playerEntity, this.m_enemyEntities, attackedIndex, this.m_abilityIndex);

            // add player status information that resulted from attacking (such as cleansed, heal received, etc.)
            this.TryAddAllyTextInformation(this.m_playerBehavior, outcome.Status, outcome.HealReceived, 0, 0);

            // this is in case currently displayed entity is an enemy entity that dies as a result of an attack, in which case we don't have to update UI since we close it
            bool isUpdatedAlready = false;

            // for each enemy on the battlefield
            for (int i = 0; i < outcome.EnemyInfos.Length; ++i)
            {
                // get the enemy outcome info
                var info = outcome.EnemyInfos[i];

                // if enemy was engaged this turn (note that this works only for valid ALIVE enemies)
                if (info.Engaged)
                {
                    // add enemy status information that resulted from attacking it (such as dispelled, missed, damage dealt, etc.)
                    this.TryAddEnemyTextInformation(this.m_enemyBehaviors[i], info.Status, info.Missed, info.DamageDealt);

                    // if enemy is killed as a result of an attack
                    if (info.Killed)
                    {
                        // log that enemy was killed
                        Debug.Log($"Enemy {i} killed!");

                        // remove enemy's health indicator from the screen since it is dead
                        this.BattleUI.RemoveHealthIndicator(this.m_enemyEntities[i]);

                        // play death animation for the enemy
                        this.m_enemyBehaviors[i].PlayAnimation(BattleBehavior.AnimationType.Death);

                        // if the currently displayed entity on the UI is this enemy, hide the UI and mark UI as updated already
                        if (this.BattleUI.CurrentEntity == this.m_enemyEntities[i])
                        {
                            isUpdatedAlready = true;

                            this.BattleUI.CurrentEntity = null;
                        }
                    }
                    else
                    {
                        // otherwise, if enemy was engaged and attack did NOT miss, play damaged animation
                        if (!info.Missed)
                        {
                            this.m_enemyBehaviors[i].PlayAnimation(BattleBehavior.AnimationType.Damaged);
                        }
                    }
                }
            }

            // if UI was not updated yet, update it
            if (!isUpdatedAlready)
            {
                this.BattleUI.UpdateInterface();
            }

            // reset ability index (even tho not really needed since actions are locked regardless)
            this.m_abilityIndex = -2;

            // wait for next frame to happen
            yield return null;

            // wait while all animations as a result of attack finished playing
            while (true)
            {
                // in the meantime, if forced exit was requested, quit the battle
                if (this.m_forceExit)
                {
                    this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                    yield break;
                }

                // otherwise check if cursor need to be updated based on where we hover
                this.MaybeChangeCursorTexture(false);

                // also change entity displayed if a different one was selected
                this.MaybeChangeEntityDisplayed();

                // if all animations finished playing, exit the loop
                if (!this.HasAnyAnimationsPlaying())
                {
                    break;
                }

                // otherwise wait for next frame to happen
                yield return null;
            }

            // wait for next frame to happen
            yield return null;

            // check as always if forced exit was requested (this is required every frame, yes)
            if (this.m_forceExit)
            {
                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                yield break;
            }

            // if we attacked any enemy, attack animation played, meaning the player needs to back off to their origin position
            if (attackedIndex >= 0)
            {
                // request player to play backoff animation
                this.m_playerBehavior.PlayAnimation(BattleBehavior.AnimationType.BackOff);

                // wait for that animation to finish playing
                while (true)
                {
                    // if force exit was requested, quit
                    if (this.m_forceExit)
                    {
                        this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                        yield break;
                    }

                    // change cursor texture as needed based on hovering
                    this.MaybeChangeCursorTexture(false);

                    // change currently displayed entity on the UI if we selected a new one
                    this.MaybeChangeEntityDisplayed();

                    // once backoff animation is done, exit the loop
                    if (!this.HasAnyAnimationsPlaying())
                    {
                        break;
                    }

                    // otherwise wait for next frame to happen
                    yield return null;
                }
            }

            // wait for next frame to occur
            yield return null;

            // check if force exit was requested
            if (this.m_forceExit)
            {
                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                yield break;
            }

            // change cursor texture based on where we are hovering
            this.MaybeChangeCursorTexture(false);

            // change the current entity displayed if we clicked on any ALIVE entity
            this.MaybeChangeEntityDisplayed();

            // now we check if any enemy is alive, and, if not, then game's over, otherwise we transfer turn to enemy
            bool isSomeoneAlive = false;

            // for each enemy in the list of enemies
            for (int i = 0; i < this.m_enemyEntities.Length; ++i)
            {
                // if enemy is alive, mark the boolean true and break
                if (this.m_enemyEntities[i].IsAlive)
                {
                    isSomeoneAlive = true;

                    break;
                }
            }

            // if no enemies are alive, we can exit the battle now non-forcefully
            if (!isSomeoneAlive)
            {
                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Victory));

                yield break;
            }

            // otherwise we wait one more frame before we transfer turn
            yield return null;

            // we check last time if forced exit was requested
            if (this.m_forceExit)
            {
                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                yield break;
            }

            // if not, then update cursor texture
            this.MaybeChangeCursorTexture(false);

            // also update the entity displayed as needed
            this.MaybeChangeEntityDisplayed();

            // and, finally, start enemy move coroutine, exiting this one
            this.m_currentRoutine = this.StartCoroutine(this.PerformEnemyMove());
        }

        private IEnumerator PerformEnemyMove()
        {
            // we simulate enemies 'thinking' by giving a 1 second delay before their moves
            const float kWaitTimeBeforeEnemyMoves = 1.0f;

            // change state to enemy move
            this.m_state = BattleState.EnemyMove;

            // check if force exit was requested
            if (this.m_forceExit)
            {
                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                yield break;
            }

            // log that enemy is moving now
            Debug.Log("Performing enemy move...");

            // update the turn label information to enemy one
            this.BattleUI.UpdateTurnLabel("ENEMY MOVE", new Color32(185, 0, 0, 255));

            // change cursor texture based on where we are hovering
            this.MaybeChangeCursorTexture(false);

            // change the current entity displayed if we clicked on any ALIVE entity
            this.MaybeChangeEntityDisplayed();

            // wait for next frame to happen
            yield return null;

            // check if force exit was requested
            if (this.m_forceExit)
            {
                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                yield break;
            }

            // change cursor texture based on where we are hovering
            this.MaybeChangeCursorTexture(false);

            // change the current entity displayed if we clicked on any ALIVE entity
            this.MaybeChangeEntityDisplayed();

            // start the move with cooldown of all enemy abilities and effects (similar to player)
            for (int i = 0; i < this.m_enemyEntities.Length; ++i)
            {
                // get enemy entity and its associated behavior
                var enemy = this.m_enemyEntities[i];
                var behavior = this.m_enemyBehaviors[i];

                // perform updates only on alive enemies (note that this is also where we decide whether enemy's move is blocked b/c of stuns)
                if (enemy.IsAlive)
                {
                    // regenerate stuff first
                    enemy.Regenerate();

                    // cooldown all enemy abilities and effects, as well as get information on heal, mana, and dmg received amount
                    enemy.Cooldown(out int coolHeal, out int coolMana, out int coolDmgs);

                    // add animated text displaying heal received, mana (no) and damage received as a result of applied effects
                    this.TryAddAllyTextInformation(behavior, null, coolHeal, coolMana, coolDmgs);

                    // if enemy is no longer alive (as a result of poison or burn)
                    if (!enemy.IsAlive)
                    {
                        // if this, now dead, enemy is currently displayed on UI, hide it
                        if (this.BattleUI.CurrentEntity == enemy)
                        {
                            this.BattleUI.CurrentEntity = null;
                        }

                        // remove health indicator for enemy since it is now dead
                        this.BattleUI.RemoveHealthIndicator(enemy);

                        // play death animation for this enemy
                        behavior.PlayAnimation(BattleBehavior.AnimationType.Death);
                    }
                    else
                    {
                        // if this enemy is currently displayed on UI, update its inforation there
                        if (this.BattleUI.CurrentEntity == enemy)
                        {
                            this.BattleUI.UpdateInterface();
                        }

                        // if any damage was received as a result of effects, play damaged animation
                        if (coolDmgs > 0)
                        {
                            behavior.PlayAnimation(BattleBehavior.AnimationType.Damaged);
                        }
                    }
                }
            }

            // wait for next frame to happen
            yield return null;

            // wait until all animations (text, damaged, death) finish playing before starting attacks
            while (true)
            {
                // if forced exit was requested, quit the battle
                if (this.m_forceExit)
                {
                    this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                    yield break;
                }

                // otherwise update cursor based on hover information
                this.MaybeChangeCursorTexture(false);

                // also update displayed entity based on click information
                this.MaybeChangeEntityDisplayed();

                // once all animations finished playing, exit the loop
                if (!this.HasAnyAnimationsPlaying())
                {
                    break;
                }

                // otherwise wait for next frame to occur
                yield return null;
            }

            // we have to ensure that there is still at least one enemy still alive before we can move (otherwise they all died b/c of effects, so game over)
            bool isSomeoneAlive = false;

            // for each enemy in the list of enemies
            for (int i = 0; i < this.m_enemyEntities.Length; ++i)
            {
                // if enemy is alive, mark the boolean true and break
                if (this.m_enemyEntities[i].IsAlive)
                {
                    isSomeoneAlive = true;

                    break;
                }
            }

            // if no enemies are alive, we can exit the battle now non-forcefully
            if (!isSomeoneAlive)
            {
                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Victory));

                yield break;
            }

            // wait for one more frame before starting attacks
            yield return null;

            // technically this should never be the case, but ensure
            if (!this.m_playerEntity.IsAlive)
            {
                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Defeat));

                yield break;
            }

            // initialize player turn for this move
            this.m_playerEntity.InitTurn();

            // we should wait before first enemy moves as well
            float delay = 0.0f;

            // loop until enough time passes so that first enemy can attack
            while (delay < kWaitTimeBeforeEnemyMoves)
            {
                // if we want to forcefully exit, do it
                if (this.m_forceExit)
                {
                    this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                    yield break;
                }

                // otherwise update cursor texture
                this.MaybeChangeCursorTexture(false);

                // as well as currently displayed entity
                this.MaybeChangeEntityDisplayed();

                // increment time passed
                delay += Time.deltaTime;

                // and wait for next frame
                yield return null;
            }

            // now, we iterate over all ALIVE and NOT stunned enemies and perform attack, as long as player is still alive that is
            for (int i = 0; i < this.m_enemyEntities.Length; ++i)
            {
                // get enemy entity and behavior instances
                var enemy = this.m_enemyEntities[i];
                var behavior = this.m_enemyBehaviors[i];

                // only perform an attack if an enemy is ALIVE and NOT stunned (hence why we apply effects before-hand)
                if (enemy.IsAlive && !enemy.TurnStats.BlockCurrentMove)
                {
                    // ensure that no forced exit was requested in the meantime
                    if (this.m_forceExit)
                    {
                        this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                        yield break;
                    }

                    // if not, update the cursor texture if we hover somewhere
                    this.MaybeChangeCursorTexture(false);

                    // also change entity displayed based on clicking
                    this.MaybeChangeEntityDisplayed();

                    // randomly get an ability or basic attack that an enemy will be using
                    int abilityIndex = BattleUtility.GetAbilityUsedByEnemy(enemy);

                    // check if that ability does any damage: if it's not a basic attack and does NO damage, we don't need to perform attack animation
                    var attackDamage = abilityIndex < 0 || enemy.Abilities[abilityIndex].DoesDamage;

                    // if an ability selected for an enemy deals damage, perform attack animation
                    if (attackDamage)
                    {
                        // launch attack animation on the enemy behavior with target being the player
                        behavior.PlayAnimation(BattleBehavior.AnimationType.Attack, this.m_playerBehavior);

                        // wait until attack animation finished playing
                        while (true)
                        {
                            // if forced exit was requested, quit the battle
                            if (this.m_forceExit)
                            {
                                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                                yield break;
                            }

                            // as always, update cursor in the meantime
                            this.MaybeChangeCursorTexture(false);

                            // as well as update displayed entity
                            this.MaybeChangeEntityDisplayed();

                            // if animation finished playing, break the loop
                            if (!this.HasAnyAnimationsPlaying())
                            {
                                break;
                            }

                            // otherwise wait for next frame
                            yield return null;
                        }
                    }

                    // wait one more frame so that all changes apply
                    yield return null;

                    // if forced exit was requested, exit the battle
                    if (this.m_forceExit)
                    {
                        this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                        yield break;
                    }

                    // update cursor based on hover information
                    this.MaybeChangeCursorTexture(false);

                    // update entity based on click information
                    this.MaybeChangeEntityDisplayed();

                    // finally, perform an attack with this enemy with the respect to player using ability selected, and get the outcome information
                    var outcome = BattleUtility.Attack(enemy, this.m_playerEntity, abilityIndex);

                    // add any animated text information to this enemy as a result of this attack (such as cleansed, heal received, etc.)
                    this.TryAddAllyTextInformation(behavior, outcome.EnemyStatus, outcome.HealReceived, 0, 0);

                    // this helps us track whether we need to update the interface later
                    bool isUpdatedAlready = false;

                    // if player was engaged in this attack (meaning it was alive before the attack)
                    if (outcome.EngagedPlayer)
                    {
                        // add any outcome text information to it (such as dispelled, missed, damage dealt, etc.)
                        this.TryAddEnemyTextInformation(this.m_playerBehavior, outcome.PlayerStatus, outcome.MissedPlayer, outcome.DamageDealt);

                        // if player was killed with this attack
                        if (outcome.KilledPlayer)
                        {
                            // log that player is dead now
                            Debug.Log("Player killed!");

                            // remove player's health indicator from the UI overlay
                            this.BattleUI.RemoveHealthIndicator(this.m_playerEntity);

                            // play death animation for the player behavior
                            this.m_playerBehavior.PlayAnimation(BattleBehavior.AnimationType.Death);

                            // if player is currently displayed on UI, hide it and mark it as updated
                            if (this.BattleUI.CurrentEntity == this.m_playerEntity)
                            {
                                isUpdatedAlready = true;

                                this.BattleUI.CurrentEntity = null;
                            }
                        }
                        else
                        {
                            // otherwise, play damage animation for player only if NOT missed
                            if (!outcome.MissedPlayer)
                            {
                                this.m_playerBehavior.PlayAnimation(BattleBehavior.AnimationType.Damaged);
                            }
                        }
                    }

                    // if UI was not updated yet, update
                    if (!isUpdatedAlready)
                    {
                        this.BattleUI.UpdateInterface();
                    }

                    // wait for next frame to occur
                    yield return null;

                    // now we wait until all text animations (and player damaged/death animation) finished playing
                    while (true)
                    {
                        // in the meantime, if forced exit was requested, quit
                        if (this.m_forceExit)
                        {
                            this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                            yield break;
                        }

                        // otherwise update cursor texture accordingly
                        this.MaybeChangeCursorTexture(false);

                        // the same applies to the currently displayed entity
                        this.MaybeChangeEntityDisplayed();

                        // once all animations finished playing, exit the loop
                        if (!this.HasAnyAnimationsPlaying())
                        {
                            break;
                        }

                        // wait for next frame and proceed again
                        yield return null;
                    }

                    // wait for next frame to happen so that changes are submitted
                    yield return null;

                    // if forced exit was requested, quit the battle
                    if (this.m_forceExit)
                    {
                        this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                        yield break;
                    }

                    // if enemy ability used performed attack animation beforehand, we have to do backoff animation
                    if (attackDamage)
                    {
                        // request backoff animation to be played for this enemy
                        behavior.PlayAnimation(BattleBehavior.AnimationType.BackOff);

                        // wait for the animation to complete
                        while (true)
                        {
                            // if forced exit was requested, exit accordingly
                            if (this.m_forceExit)
                            {
                                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                                yield break;
                            }

                            // update cursor texture if we're still here
                            this.MaybeChangeCursorTexture(false);

                            // update displayed entity if we're still here
                            this.MaybeChangeEntityDisplayed();

                            // once animation finished playing, continue to next segment
                            if (!this.HasAnyAnimationsPlaying())
                            {
                                break;
                            }

                            // otherwise wait for next frame
                            yield return null;
                        }
                    }

                    // wait one more frame before proceeding
                    yield return null;

                    // if playe is no longer alive, we exit the battle non-forcefully (no need for other enemies to perform attacks anymore since no more targets)
                    if (!this.m_playerEntity.IsAlive)
                    {
                        this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Defeat));

                        yield break;
                    }

                    // if forced exit was requested, quit the battle
                    if (this.m_forceExit)
                    {
                        this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                        yield break;
                    }

                    // otherwise update cursor texture based on hover
                    this.MaybeChangeCursorTexture(false);

                    // also update displayed entity based on click
                    this.MaybeChangeEntityDisplayed();

                    // wait one more frame before starting delaying
                    yield return null;

                    // this tracks the duration passed so far (note: cannot use WaitForSeconds because we still have to update cursor and UI information)
                    float elapsed = 0.0f;

                    // while delay and animations (should be no animations, but still) haven't finished yet
                    while (true)
                    {
                        // if forced exit was requested, abort the battle
                        if (this.m_forceExit)
                        {
                            this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                            yield break;
                        }

                        // while waiting, update any cursor changes
                        this.MaybeChangeCursorTexture(false);

                        // while waiting, update any entity changes
                        this.MaybeChangeEntityDisplayed();

                        // increase time elapsed by delta time of previous frame
                        elapsed += Time.deltaTime;

                        // if no animations are playing, and enough time has passed since enemy's attack, we can continue
                        if (!this.HasAnyAnimationsPlaying() && elapsed >= kWaitTimeBeforeEnemyMoves)
                        {
                            break;
                        }

                        // otherwise we wait for next frame
                        yield return null;
                    }
                }
            }

            // wait for next frame to happen
            yield return null;

            // note: playe SHOULD be alive at this point b/c we would've game over-ed either before previous loop or during it
            if (this.m_forceExit)
            {
                this.m_currentRoutine = this.StartCoroutine(this.PerformBattleFinal(BattleOutcome.Exit));

                yield break;
            }

            // last time check if cursor needs to be updated
            this.MaybeChangeCursorTexture(false);

            // last time check if displayed entity needs to be updated
            this.MaybeChangeEntityDisplayed();

            // finally, start player move coroutine, exiting this one
            this.m_currentRoutine = this.StartCoroutine(this.PerformPlayerMove());
        }
        
        

        private void SetupCallbacks(bool attach)
        {
            if (attach)
            {
                this.BattleUI.OnUseAttackRequest += OnUseAttackRequest;

                this.BattleUI.OnUseAbilityRequest += OnUseAbilityRequest;

                this.BattleUI.OnUsePotionRequest += OnUsePotionRequest;

                this.BattleUI.OnCancelInteractRequest += OnCancelInteractRequest;

                this.BattleUI.OnExitRequest += OnExitRequest;

                ScreenManager.Instance.OnScreenResolutionChanged += OnScreenResolutionChanged;
            }
            else
            {
                this.BattleUI.OnUseAttackRequest -= OnUseAttackRequest;

                this.BattleUI.OnUseAbilityRequest -= OnUseAbilityRequest;

                this.BattleUI.OnUsePotionRequest -= OnUsePotionRequest;

                this.BattleUI.OnCancelInteractRequest -= OnCancelInteractRequest;

                this.BattleUI.OnExitRequest -= OnExitRequest;

                ScreenManager.Instance.OnScreenResolutionChanged -= OnScreenResolutionChanged;
            }

            void OnUseAttackRequest()
            {
                this.m_abilityIndex = -1;
            }

            void OnUseAbilityRequest(int index)
            {
                this.m_abilityIndex = index;
            }

            void OnUsePotionRequest(int index)
            {
                var player = this.m_playerEntity;

                if (player is not null)
                {
                    int currentHP = player.EntityStats.CurHealth;
                    int currentMP = player.EntityStats.CurMana;
                    int effectNum = player.Effects.Count;

                    player.UsePotion(index);

                    this.TryAddAllyTextInformation(this.m_playerBehavior, null, player.EntityStats.CurHealth - currentHP, player.EntityStats.CurMana - currentMP, 0);

                    if (effectNum < player.Effects.Count)
                    {
                        // #TODO add to effect list?
                    }

                    this.BattleUI.ConfirmActionFinished();

                    this.BattleUI.UpdateInterface();
                }
            }

            void OnCancelInteractRequest()
            {
                this.m_abilityIndex = -2;
            }

            void OnExitRequest()
            {
                this.m_forceExit = true;
            }

            void OnScreenResolutionChanged()
            {
                if (this.m_playerBehavior != null)
                {
                    this.m_playerBehavior.RecalculateTransform();
                }

                if (this.m_enemyBehaviors is not null)
                {
                    for (int i = 0; i < this.m_enemyBehaviors.Length; ++i)
                    {
                        if (this.m_enemyBehaviors[i] != null)
                        {
                            this.m_enemyBehaviors[i].RecalculateTransform();
                        }
                    }
                }

                if (this.m_background != null)
                {
                    var scaleY = ScreenManager.Instance.OrthographicSize * 0.4f; // (orthoSize / 5) * 2

                    this.m_background.gameObject.transform.localScale = new Vector3(scaleY * ScreenManager.Instance.AspectRatio * this.m_backgroundRatio, scaleY, 1.0f);
                }
            }
        }

        private bool HasAnyAnimationsPlaying()
        {
            if (this.BattleUI.HasAnyAnimationsPlaying)
            {
                return true;
            }

            if (this.m_playerBehavior.Current is not BattleBehavior.AnimationType.None and not BattleBehavior.AnimationType.Idle)
            {
                return true;
            }

            for (int i = 0; i < this.m_enemyBehaviors.Length; ++i)
            {
                if (this.m_enemyBehaviors[i].Current is not BattleBehavior.AnimationType.None and not BattleBehavior.AnimationType.Idle)
                {
                    return true;
                }
            }

            return false;
        }

        private void MaybeChangeCursorTexture(bool forceDefault)
        {
            if (!forceDefault && InputProcessor.Instance.IsPointerOverCollider(out var collider))
            {
                var behavior = collider.GetComponent<BattleBehavior>();

                if (behavior.Entity.IsPlayer)
                {
                    ScreenManager.Instance.SetCursorTexture(ResourceManager.PlayerCursor, Vector2.zero);
                }
                else
                {
                    ScreenManager.Instance.SetCursorTexture(ResourceManager.EnemyCursor, Vector2.zero);
                }
            }
            else
            {
                ScreenManager.Instance.SetCursorTexture(ResourceManager.DefaultCursor, Vector2.zero);
            }
        }

        private void MaybeChangeCursorToTarget(bool playerIsTarget)
        {
            if (InputProcessor.Instance.IsPointerOverCollider(out var collider))
            {
                var behavior = collider.GetComponent<BattleBehavior>();

                if (behavior.Entity.IsPlayer == playerIsTarget)
                {
                    var texture = ResourceManager.TargetCursor;

                    ScreenManager.Instance.SetCursorTexture(texture, new Vector2(texture.width * 0.5f, texture.height * 0.5f));

                    return;
                }
            }

            ScreenManager.Instance.SetCursorTexture(ResourceManager.DefaultCursor, Vector2.zero);
        }

        private void MaybeHighlightTargetEnemies(bool playerIsTarget)
        {
            this.ForceUnhighlightAll();

            if (InputProcessor.Instance.IsPointerOverCollider(out var collider))
            {
                var behavior = collider.GetComponent<BattleBehavior>();

                if (behavior.Entity.IsPlayer == playerIsTarget)
                {
                    behavior.SetGlowHighlight(true, Color.red);
                }
            }
        }

        private void MaybeChangeEntityDisplayed()
        {
            if (InputProcessor.Instance.RaycastLeftClickIfHappened(out var collider))
            {
                if (collider != null && collider.gameObject.TryGetComponent<BattleBehavior>(out var behavior) && behavior.Entity.IsAlive)
                {
                    this.BattleUI.CurrentEntity = behavior.Entity;
                }
                else
                {
                    this.BattleUI.CurrentEntity = null;
                }
            }
        }

        private void ForceUnhighlightAll()
        {
            this.m_playerBehavior.SetGlowHighlight(false, Color.clear);

            for (int i = 0; i < this.m_enemyBehaviors.Length; ++i)
            {
                this.m_enemyBehaviors[i].SetGlowHighlight(false, Color.clear);
            }
        }

        private void TryAddAllyTextInformation(BattleBehavior behavior, string[] status, int healAmount, int manaAmount, int dmgsAmount)
        {
            float delay = 0.0f;

            if (status is not null)
            {
                for (int i = 0; i < status.Length; ++i)
                {
                    this.BattleUI.AddAnimatedText(status[i], 1.4f, delay, 30, 1, new Color(215, 215, 215, 255), behavior.UnitPosition, behavior.TopMostPoint);

                    delay += 1.0f;
                }
            }

            if (healAmount > 0)
            {
                this.BattleUI.AddAnimatedText("+" + healAmount.ToString(), 1.4f, delay, 60, 2, new Color32(0, 200, 0, 255), behavior.UnitPosition, behavior.TopMostPoint);

                delay += 1.0f;
            }

            if (manaAmount > 0)
            {
                this.BattleUI.AddAnimatedText("+" + manaAmount.ToString(), 1.4f, delay, 60, 2, new Color(0, 128, 255, 255), behavior.UnitPosition, behavior.TopMostPoint);

                delay += 1.0f;
            }

            if (dmgsAmount > 0)
            {
                this.BattleUI.AddAnimatedText("-" + dmgsAmount.ToString(), 1.4f, delay, 60, 2, new Color(200, 0, 0, 255), behavior.UnitPosition, behavior.TopMostPoint);
            }
        }

        private void TryAddEnemyTextInformation(BattleBehavior behavior, string[] status, bool missed, int damageDealt)
        {
            float delay = 0.0f;

            if (status is not null)
            {
                for (int i = 0; i < status.Length; ++i)
                {
                    this.BattleUI.AddAnimatedText(status[i], 1.4f, delay, 30, 1, new Color(215, 215, 215, 255), behavior.UnitPosition, behavior.TopMostPoint);

                    delay += 1.0f;
                }
            }

            if (missed)
            {
                this.BattleUI.AddAnimatedText("MISSED", 1.4f, delay, 30, 1, new Color(215, 215, 215, 255), behavior.UnitPosition, behavior.TopMostPoint);
            }
            else
            {
                this.BattleUI.AddAnimatedText("-" + damageDealt.ToString(), 1.4f, delay, 60, 2, new Color(200, 0, 0, 255), behavior.UnitPosition, behavior.TopMostPoint);
            }
        }
    }
}
