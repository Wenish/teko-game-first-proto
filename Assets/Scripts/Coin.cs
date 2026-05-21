using UnityEngine;
using VContainer;

public class Coin : MonoBehaviour
{
    [SerializeField] private int value = 1;

    private CoinService _coinService;

    [Inject]
    public void Construct(CoinService coinService)
    {
        _coinService = coinService;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<PlayerMovement>(out _))
            return;

        _coinService.AddCoin(value);
        Destroy(gameObject);
    }
}