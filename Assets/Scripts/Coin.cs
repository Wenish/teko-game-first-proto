using MessagePipe;
using UnityEngine;
using VContainer;

public class Coin : MonoBehaviour
{
    [SerializeField] private int value = 1;

     private IPublisher<CoinCollectedEvent> _coinCollectedPublisher;

    [Inject]
    public void Construct(IPublisher<CoinCollectedEvent> coinCollectedPublisher)
    {
        _coinCollectedPublisher = coinCollectedPublisher;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<PlayerMovement>(out _))
            return;

        _coinCollectedPublisher.Publish(new CoinCollectedEvent(value));
        
        Destroy(gameObject);
    }
}