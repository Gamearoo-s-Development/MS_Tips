using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace MSTips;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class RandomTipsPlugin : BaseUnityPlugin
{
    private const string PluginGuid = "com.gamearoo.megastore.randomtips";
    private const string PluginName = "Megastore Checkout Tipping";
    private const string PluginVersion = "1.2.0";

    private ConfigEntry<int>? _minTipAmount;
    private ConfigEntry<int>? _maxTipAmount;
    private ConfigEntry<float>? _baseTipChance;
    private ConfigEntry<float>? _serviceScoreWeight;
    private ConfigEntry<float>? _streakBonusPerCheckout;
    private ConfigEntry<int>? _maxNoTipCheckouts;
    private ConfigEntry<bool>? _debugLog;
    private ConfigEntry<bool>? _showTipToast;
    private ConfigEntry<float>? _tipToastSeconds;

    private float _recentNegativeSignals;
    private float _recentPositiveSignals;
    private GUIStyle? _toastStyle;
    private string _tipToastText = string.Empty;
    private float _tipToastHideTime;
    private int _sessionTipTotal;
    private int _noTipCheckoutStreak;

    private void Awake()
    {
        _minTipAmount = Config.Bind("Tipping", "MinTipAmountDollars", 0, "Minimum tip amount (in dollars) when a customer decides to tip.");
        _maxTipAmount = Config.Bind("Tipping", "MaxTipAmountDollars", 5, "Maximum tip amount (in dollars) when a customer decides to tip.");
        _baseTipChance = Config.Bind("Tipping", "BaseTipChance", 0.12f, "Base chance for a tip on checkout (0.0 to 1.0).");
        _serviceScoreWeight = Config.Bind("Tipping", "ServiceScoreWeight", 0.60f, "How much service score affects tip chance (0.0 to 1.0).");
        _streakBonusPerCheckout = Config.Bind("Tipping", "NoTipStreakBonusPerCheckout", 0.03f, "Extra tip chance added for each checkout without a tip.");
        _maxNoTipCheckouts = Config.Bind("Tipping", "MaxNoTipCheckouts", 8, "Guarantee a tip after this many no-tip checkouts.");
        _debugLog = Config.Bind("Debug", "VerboseLog", false, "Log quality score and tip roll details.");
        _showTipToast = Config.Bind("Display", "ShowTipToast", true, "Show an on-screen message when a tip is applied.");
        _tipToastSeconds = Config.Bind("Display", "TipToastSeconds", 2.5f, "How long the tip message stays on-screen.");

        EventManager.AddListener<float>(PaymentEvents.CASH_PAYMENT_DONE, OnCheckoutPaid);
        EventManager.AddListener<float>(PaymentEvents.POS_PAYMENT_DONE, OnCheckoutPaid);

        EventManager.AddListener<Customer>(OccasionEvents.ITEM_OUT_OF_STOCK, OnNegativeOccasion);
        EventManager.AddListener<Customer>(OccasionEvents.CHECKOUT_LONG_WAIT, OnNegativeOccasion);
        EventManager.AddListener<Customer>(OccasionEvents.BAKERY_FRESHNESS_CONCERN, OnNegativeOccasion);
        EventManager.AddListener<Customer>(OccasionEvents.REGISTER_GREETING, OnPositiveOccasion);

        EventManager.AddListener(ReviewEvents.PAYMENTS_FAILED_REVIEW, OnPaymentFailedReview);
        EventManager.AddListener(ReviewEvents.PAYMENTS_SUCCESS_REVIEW, OnPaymentSuccessReview);

        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<float>(PaymentEvents.CASH_PAYMENT_DONE, OnCheckoutPaid);
        EventManager.RemoveListener<float>(PaymentEvents.POS_PAYMENT_DONE, OnCheckoutPaid);

        EventManager.RemoveListener<Customer>(OccasionEvents.ITEM_OUT_OF_STOCK, OnNegativeOccasion);
        EventManager.RemoveListener<Customer>(OccasionEvents.CHECKOUT_LONG_WAIT, OnNegativeOccasion);
        EventManager.RemoveListener<Customer>(OccasionEvents.BAKERY_FRESHNESS_CONCERN, OnNegativeOccasion);
        EventManager.RemoveListener<Customer>(OccasionEvents.REGISTER_GREETING, OnPositiveOccasion);

        EventManager.RemoveListener(ReviewEvents.PAYMENTS_FAILED_REVIEW, OnPaymentFailedReview);
        EventManager.RemoveListener(ReviewEvents.PAYMENTS_SUCCESS_REVIEW, OnPaymentSuccessReview);
    }

    private void OnNegativeOccasion(Customer _)
    {
        _recentNegativeSignals = Mathf.Min(20f, _recentNegativeSignals + 1f);
    }

    private void OnPositiveOccasion(Customer _)
    {
        _recentPositiveSignals = Mathf.Min(20f, _recentPositiveSignals + 1f);
    }

    private void OnPaymentFailedReview()
    {
        _recentNegativeSignals = Mathf.Min(20f, _recentNegativeSignals + 2f);
    }

    private void OnPaymentSuccessReview()
    {
        _recentPositiveSignals = Mathf.Min(20f, _recentPositiveSignals + 2f);
    }

    private void OnCheckoutPaid(float checkoutAmount)
    {
        if (checkoutAmount <= 0f)
        {
            return;
        }

        var (minTip, maxTip) = GetTipRange();
        if (maxTip <= 0)
        {
            return;
        }

        var stockScore = GetStockAvailabilityScore();
        var negativePenalty = Mathf.Clamp01(_recentNegativeSignals / 12f);
        var positiveBoost = Mathf.Clamp01(_recentPositiveSignals / 12f);

        var serviceScore = Mathf.Clamp01(0.15f + stockScore * 0.65f + positiveBoost * 0.30f - negativePenalty * 0.45f);
        var baseChance = Mathf.Clamp01(_baseTipChance?.Value ?? 0.12f);
        var serviceWeight = Mathf.Clamp01(_serviceScoreWeight?.Value ?? 0.60f);
        var streakBonus = Mathf.Max(0f, _streakBonusPerCheckout?.Value ?? 0.03f) * _noTipCheckoutStreak;
        var tipChance = Mathf.Clamp01(baseChance + serviceScore * serviceWeight + streakBonus);
        var guaranteeAfter = Mathf.Max(1, _maxNoTipCheckouts?.Value ?? 8);
        var forcedTip = _noTipCheckoutStreak >= guaranteeAfter;

        if (!forcedTip && Random.value > tipChance)
        {
            _noTipCheckoutStreak++;
            DecaySignals();
            if (_debugLog?.Value == true)
            {
                Logger.LogInfo($"No tip this checkout. score={serviceScore:F2}, stock={stockScore:F2}, chance={tipChance:F2}, streak={_noTipCheckoutStreak}/{guaranteeAfter}");
            }
            return;
        }

        var bias = Mathf.Pow(Random.value, Mathf.Lerp(2.2f, 0.85f, serviceScore));
        var tipAmount = Mathf.RoundToInt(Mathf.Lerp(minTip, maxTip, bias));

        if (forcedTip && tipAmount <= 0 && maxTip > 0)
        {
            tipAmount = Mathf.Max(1, minTip);
        }

        if (tipAmount > 0)
        {
            EventManager.NotifyEvent<float>(EconomyEvents.ADD_SOFT_CURRENCY, tipAmount);
            _sessionTipTotal += tipAmount;
            _noTipCheckoutStreak = 0;
            Logger.LogInfo(forcedTip
                ? $"Customer tip received (streak guarantee): ${tipAmount:0.00}"
                : $"Customer tip received: ${tipAmount:0.00}");
            ShowTipToast(tipAmount);
        }
        else if (_debugLog?.Value == true)
        {
            _noTipCheckoutStreak++;
            Logger.LogInfo("Customer decided not to tip ($0).");
        }

        DecaySignals();
    }

    private void DecaySignals()
    {
        _recentNegativeSignals *= 0.72f;
        _recentPositiveSignals *= 0.72f;
    }

    private (int min, int max) GetTipRange()
    {
        var min = Mathf.Max(0, _minTipAmount?.Value ?? 0);
        var max = Mathf.Max(0, _maxTipAmount?.Value ?? 5);

        if (max < min)
        {
            max = min;
            Logger.LogWarning("MaxTipAmountDollars was less than MinTipAmountDollars. Using MinTipAmountDollars for both values.");
        }

        return (min, max);
    }

    private float GetStockAvailabilityScore()
    {
        var stockManager = SingletonBehaviour<StockManager>.Instance;
        if (stockManager == null)
        {
            return 0.5f;
        }

        var purchasableProducts = stockManager.PurchasableProducts;
        if (purchasableProducts == null || purchasableProducts.Count == 0)
        {
            return 0.5f;
        }

        var considered = 0;
        var available = 0;
        var sampleSize = Mathf.Min(purchasableProducts.Count, 40);

        for (var index = 0; index < sampleSize; index++)
        {
            var productType = purchasableProducts[index];
            var totalAvailable = stockManager.GetAvailableStockOnShelves(productType) + stockManager.GetAvailableStockInBoxes(productType);
            considered++;
            if (totalAvailable > 0)
            {
                available++;
            }
        }

        if (considered == 0)
        {
            return 0.5f;
        }

        return (float)available / considered;
    }

    private void ShowTipToast(int amount)
    {
        if (_showTipToast?.Value != true)
        {
            return;
        }

        _tipToastText = $"<b>TIP RECEIVED</b>  +${amount}\nTips Total: ${_sessionTipTotal}";
        _tipToastHideTime = Time.realtimeSinceStartup + Mathf.Max(0.5f, _tipToastSeconds?.Value ?? 2.5f);
    }

    private void OnGUI()
    {
        if (_toastStyle == null)
        {
            _toastStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                richText = true,
                wordWrap = true,
                padding = new RectOffset(12, 12, 8, 8)
            };
        }

        if (string.IsNullOrEmpty(_tipToastText) || Time.realtimeSinceStartup >= _tipToastHideTime)
        {
            return;
        }

        var width = Mathf.Min(Screen.width - 32f, 540f);
        var x = (Screen.width - width) * 0.5f;
        var rect = new Rect(x, 24f, width, 84f);
        GUI.Box(rect, _tipToastText, _toastStyle);
    }
}