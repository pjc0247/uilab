//Dependencies:
// - Scroller: Source > Scripts > HelperClasses
// - DirectionalScrollSnapEditor: Source > Editor (optional)

//Contributors:
//BeksOmega

using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.UI.ScrollSnaps
{
    [HelpURL("https://bitbucket.org/beksomega/unityuiscrollsnaps/wiki/Components/DirectionalScrollSnap")]
    [AddComponentMenu("UI/Scroll Snaps/Directional Scroll Snap")]
    [ExecuteInEditMode]
    public class DirectionalScrollSnap : UIBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, ICanvasElement, IScrollHandler, ILayoutGroup
    {

        #region Variables
        public enum MovementDirection
        {
            Horizontal,
            Vertical
        }

        public enum MovementType
        {
            Clamped,
            Elastic
        }

        public enum SnapType
        {
            SnapToNearest,
            SnapToLastPassed,
            SnapToNext
        }

        private enum Direction
        {
            TowardsStart,
            TowardsEnd
        }

        public enum InterpolatorType
        {
            Accelerate,
            AccelerateDecelerate,
            Anticipate,
            AnticipateOvershoot,
            Decelerate,
            DecelerateAccelerate,
            Linear,
            Overshoot,
            ViscousFluid,
        }

        public enum FilterMode
        {
            BlackList,
            WhiteList
        }

        public enum StartMovementEventType
        {
            Touch,
            ScrollBar,
            OnScroll,
            ButtonPress,
            Programmatic
        }
        
        public enum LockMode
        {
            Before,
            After,
        }

        [Serializable]
        public class Vector2Event : UnityEvent<Vector2> { }
        [Serializable]
        public class StartMovementEvent : UnityEvent<StartMovementEventType> { }
        [Serializable]
        public class IntEvent : UnityEvent<int> { }

        [SerializeField]
        private RectTransform m_Content;
        public RectTransform content
        {
            get
            {
                return m_Content;
            }
            set
            {
                m_Content = value;

                if (contentIsHorizonalLayoutGroup)
                {
                    m_MovementDirection = MovementDirection.Horizontal;
                }

                if (contentIsVerticalLayoutGroup)
                {
                    m_MovementDirection = MovementDirection.Vertical;
                }

                if (contentIsLayoutGroup && m_LayoutGroup.enabled)
                {
                    RebuildLayoutGroups();
                }
                if (contentIsLayoutGroup && Application.isPlaying)
                {
                    m_LayoutGroupWasEnabled = m_LayoutGroup.enabled;
                    m_LayoutGroup.enabled = false;
                }

                m_CalculateDistances.Clear();
                m_SnapDistances.Clear();
                UpdatePrevData();
                UpdateLayout();
            }
        }

        [SerializeField]
        private MovementType m_MovementType;
        public MovementType movementType { get { return m_MovementType; } set { m_MovementType = value; } }

        [SerializeField]
        private MovementDirection m_MovementDirection;
        public MovementDirection movementDirection
        {
            get
            {
                return m_MovementDirection;
            }
            set
            {
                if (contentIsHorizonalLayoutGroup)
                {
                    m_MovementDirection = MovementDirection.Horizontal;
                }
                else if (contentIsVerticalLayoutGroup)
                {
                    m_MovementDirection = MovementDirection.Vertical;
                }
                else
                {
                    m_MovementDirection = value;
                }
            }
        }

        [SerializeField]
        private bool m_LockOtherDirection = true;
        public bool lockNonScrollingDirection { get { return m_LockOtherDirection; } set { m_LockOtherDirection = value; } }

        [SerializeField]
        private bool m_Loop = false;
        public bool loop { get { return loop; } set { m_Loop = value; } }

        [SerializeField]
        private int m_EndSpacing;
        public int endSpacing { get { return m_EndSpacing; } }

        [SerializeField]
        private bool m_SimulateFlings = true;
        public bool simulateFlings { get { return m_SimulateFlings; } set { m_SimulateFlings = value; } }

        [SerializeField]
        [Range(0.1f, .999f)]
        private float m_Friction = .8f;
        public float friction
        {
            get
            {
                return m_Friction;
            }
            set
            {
                if (m_ScrollBarFriction == m_Friction)
                {
                    m_ScrollBarFriction = value;
                }
                if (m_ScrollFriction == m_Friction)
                {
                    m_ScrollFriction = value;
                }

                m_Friction = value;
            }
        }

        [SerializeField]
        private SnapType m_SnapType = SnapType.SnapToNearest;
        public SnapType snapType { get { return m_SnapType; } set { m_SnapType = value; } }

        [SerializeField]
        private InterpolatorType m_InterpolatorType = InterpolatorType.Decelerate;
        public InterpolatorType interpolator
        {
            get
            {
                return m_InterpolatorType;
            }
            set
            {
                if (m_ButtonInterpolator == m_InterpolatorType)
                {
                    m_ButtonInterpolator = value;
                }
                if (m_ScrollBarInterpolator == m_InterpolatorType)
                {
                    m_ScrollBarInterpolator = value;
                }
                if (m_ScrollInterpolator == m_InterpolatorType)
                {
                    m_ScrollInterpolator = value;
                }

                m_InterpolatorType = value;
            }
        }

        [SerializeField]
        private float m_Tension = 2f;
        public float tension
        {
            get
            {
                return m_Tension;
            }
            set
            {
                if (m_ButtonInterpolator == m_InterpolatorType && m_ButtonTension == m_Tension)
                {
                    m_ButtonTension = value;
                }
                if (m_ScrollBarInterpolator == m_InterpolatorType && m_ScrollBarTension == m_Tension)
                {
                    m_ScrollBarTension = value;
                }
                if (m_ScrollInterpolator == m_InterpolatorType && m_ScrollTension == m_Tension)
                {
                    m_ScrollTension = value;
                }
                m_Tension = value;
            }
        }

        [SerializeField]
        private float m_MinDuration = .25f;
        public float minDuration { get { return m_MinDuration; } set { m_MinDuration = value; } }

        [SerializeField]
        private float m_MaxDuration = 2f;
        public float maxDuration { get { return m_MaxDuration; } set { m_MaxDuration = value; } }

        [SerializeField]
        private bool m_AllowTouchInput = true;
        public bool allowTouchInput {  get { return m_AllowTouchInput; } set { m_AllowTouchInput = value; } }

        [SerializeField]
        private float m_ButtonAnimationDuration = 1f;
        public float buttonAnimationDuration { get { return m_ButtonAnimationDuration; } set { m_ButtonAnimationDuration = value; } }

        [SerializeField]
        private int m_ButtonItemsToMoveBy = 1;
        public int buttonItemsToMoveBy {  get { return m_ButtonItemsToMoveBy; } set { m_ButtonItemsToMoveBy = value; } }

        [SerializeField]
        private bool m_ButtonAlwaysGoToEnd = true;
        public bool buttonAlwaysGoToEnd {  get { return m_ButtonAlwaysGoToEnd; } set { m_ButtonAlwaysGoToEnd = value; } }

        [SerializeField]
        private InterpolatorType m_ButtonInterpolator = InterpolatorType.Decelerate;
        public InterpolatorType buttonInterpolator { get { return m_ButtonInterpolator; } set { m_ButtonInterpolator = value; } }

        [SerializeField]
        private float m_ButtonTension = 2f;
        public float buttonTension { get { return m_ButtonTension; } set { m_ButtonTension = value; } }

        [SerializeField]
        [Range(0.1f, .999f)]
        private float m_ScrollBarFriction = .8f;
        public float scrollBarFriction { get { return m_ScrollBarFriction; } set { m_ScrollBarFriction = value; } }

        [SerializeField]
        private InterpolatorType m_ScrollBarInterpolator = InterpolatorType.Decelerate;
        public InterpolatorType scrollBarInterpolator { get { return m_ScrollBarInterpolator; } set { m_ScrollBarInterpolator = value; } }

        [SerializeField]
        private float m_ScrollBarTension = 2f;
        public float scrollBarTension { get { return m_ScrollBarTension; } set { m_ScrollBarTension = value; } }

        [SerializeField]
        private float m_ScrollSensitivity = 1.0f;
        public float scrollSensitivity { get { return m_ScrollSensitivity; } set { m_ScrollSensitivity = value; } }

        [SerializeField]
        private float m_ScrollDelay = .02f;
        public float scrollDelay
        {
            get
            {
                return m_ScrollDelay;
            }
            set
            {
                m_ScrollDelay = Mathf.Max(value, 0);
            }
        }

        [SerializeField]
        [Range(0.1f, .999f)]
        private float m_ScrollFriction = .8f;
        public float scrollFriction { get { return m_ScrollFriction; } set { m_ScrollFriction = value; } }

        [SerializeField]
        private InterpolatorType m_ScrollInterpolator = InterpolatorType.Decelerate;
        public InterpolatorType scrollInterpolator { get { return m_ScrollInterpolator; } set { m_ScrollInterpolator = value; } }

        [SerializeField]
        private float m_ScrollTension = 2f;
        public float scrollTension { get { return m_ScrollTension; } set { m_ScrollTension = value; } }

        [SerializeField]
        private bool m_AddInactiveChildrenToCalculatingFilter;
        public bool addInactiveChildrenToCalculatingFilter{ get { return m_AddInactiveChildrenToCalculatingFilter; } }

        [SerializeField]
        private FilterMode m_FilterModeForCalculatingSize;
        public FilterMode calculateSizeFilterMode { get { return m_FilterModeForCalculatingSize; }}

        [SerializeField]
        private List<RectTransform> m_CalculatingFilter = new List<RectTransform>();
        public List<RectTransform> calculatingFilter { get { return m_CalculatingFilter; }}

        [SerializeField]
        private bool m_AddInactiveChildrenToSnapPositionsFilter;
        public bool addInactiveChildrenToSnapPositionsFilter { get { return m_AddInactiveChildrenToSnapPositionsFilter; }}

        [SerializeField]
        private FilterMode m_FilterModeForSnapPositions;
        public FilterMode snapPositionsFilterMode { get { return m_FilterModeForSnapPositions; } }

        [SerializeField]
        private List<RectTransform> m_SnapPositionsFilter = new List<RectTransform>();
        public List<RectTransform> snapPositionsFilter { get { return m_SnapPositionsFilter; } }

        [SerializeField]
        private RectTransform m_StartItem;
        public RectTransform startItem { get { return m_StartItem; } set { m_StartItem = value; } }

        [SerializeField]
        private RectTransform m_Viewport;
        public RectTransform viewport { get { return m_Viewport; } set { m_Viewport = value; SetDirtyCaching(); } }

        private ScrollBarEventsListener m_HorizontalScrollbarEventsListener;
        [SerializeField]
        private Scrollbar m_HorizontalScrollbar;
        public Scrollbar horizontalScrollbar
        {
            get
            {
                return m_HorizontalScrollbar;
            }
            set
            {
                if (m_HorizontalScrollbar)
                {
                    m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
                    if (m_HorizontalScrollbarEventsListener)
                    {
                        m_HorizontalScrollbarEventsListener.onPointerDown -= ScrollBarPointerDown;
                        m_HorizontalScrollbarEventsListener.onPointerUp -= ScrollBarPointerUp;
                        DestroyImmediate(m_HorizontalScrollbarEventsListener);
                    }
                }

                if (!m_Loop)
                {
                    m_HorizontalScrollbar = value;
                    if (m_HorizontalScrollbar)
                    {
                        m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
                        m_HorizontalScrollbarEventsListener = m_HorizontalScrollbar.gameObject.AddComponent<ScrollBarEventsListener>();
                        m_HorizontalScrollbarEventsListener.onPointerDown += ScrollBarPointerDown;
                        m_HorizontalScrollbarEventsListener.onPointerUp += ScrollBarPointerUp;
                    }
                }
            }
        }

        private ScrollBarEventsListener m_VerticalScrollBarEventsListener;
        [SerializeField]
        private Scrollbar m_VerticalScrollbar;
        public Scrollbar verticalScrollbar
        {
            get
            {
                return m_VerticalScrollbar;
            }
            set
            {
                if (m_VerticalScrollbar)
                {
                    m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
                    if (m_VerticalScrollBarEventsListener)
                    {
                        m_VerticalScrollBarEventsListener.onPointerDown -= ScrollBarPointerDown;
                        m_VerticalScrollBarEventsListener.onPointerUp -= ScrollBarPointerUp;
                        DestroyImmediate(m_VerticalScrollBarEventsListener);
                    }
                }

                if (!m_Loop)
                {
                    m_VerticalScrollbar = value;
                    if (m_VerticalScrollbar)
                    {
                        m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
                        m_VerticalScrollBarEventsListener = m_VerticalScrollbar.gameObject.AddComponent<ScrollBarEventsListener>();
                        m_VerticalScrollBarEventsListener.onPointerDown += ScrollBarPointerDown;
                        m_VerticalScrollBarEventsListener.onPointerUp += ScrollBarPointerUp;
                    }
                }
            }
        }

        [SerializeField]
        private Button m_BackButton;
        public Button backButton
        {
            get
            {
                return m_BackButton;
            }
            set
            {
                if (m_BackButton)
                {
                    m_BackButton.onClick.RemoveListener(OnBack);
                }
                m_BackButton = value;
                if (m_BackButton)
                {
                    m_BackButton.onClick.AddListener(OnBack);
                }
            }
        }

        [SerializeField]
        private Button m_ForwardButton;
        public Button forwardButton
        {
            get
            {
                return m_ForwardButton;
            }
            set
            {
                if (m_ForwardButton)
                {
                    m_ForwardButton.onClick.RemoveListener(OnForward);
                }
                m_ForwardButton = value;
                if (m_ForwardButton)
                {
                    m_ForwardButton.onClick.AddListener(OnForward);
                }
            }
        }

        [SerializeField]
        private bool m_DrawGizmos = false;

        [SerializeField]
        private Vector2Event m_OnValueChanged = new Vector2Event();
        public Vector2Event onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        [SerializeField]
        private StartMovementEvent m_StartMovementEvent = new StartMovementEvent();
        public StartMovementEvent startMovementEvent { get { return m_StartMovementEvent; } set { m_StartMovementEvent = value; } }

        [SerializeField]
        private IntEvent m_ClosestSnapPositionChanged = new IntEvent();
        public IntEvent closestSnapPositionChanged { get { return m_ClosestSnapPositionChanged; } set { m_ClosestSnapPositionChanged = value; } }

        [SerializeField]
        private IntEvent m_SnappedToItem = new IntEvent();
        public IntEvent snappedToItem { get { return m_SnappedToItem; } set { m_SnappedToItem = value; } }

        [SerializeField]
        private IntEvent m_TargetItemSelected = new IntEvent();
        public IntEvent targetItemSelected { get { return m_TargetItemSelected; } set { m_TargetItemSelected = value; } }


        public float horizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if (m_ContentBounds.size.x <= m_ViewBounds.size.x)
                {
                    return (m_ViewBounds.min.x > m_ContentBounds.min.x) ? 1 : 0;
                }
                return (m_ViewBounds.min.x - m_ContentBounds.min.x) / (m_ContentBounds.size.x - m_ViewBounds.size.x);
            }
            set
            {
                SetNormalizedPosition(value, 0);
            }
        }

        public float verticalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if (m_ContentBounds.size.y <= m_ViewBounds.size.y)
                {
                    return (m_ViewBounds.min.y > m_ContentBounds.min.y) ? 0 : 1;
                }
                return 1 - (m_ViewBounds.min.y - m_ContentBounds.min.y) / (m_ContentBounds.size.y - m_ViewBounds.size.y);
            }
            set
            {
                SetNormalizedPosition(value, 1);
            }
        }

        private Vector2 m_Velocity;
        public Vector2 velocity { get { return m_Velocity; } }

        private int m_ClosestSnapPositionIndex;
        public int closestSnapPositionIndex { get { return m_ClosestSnapPositionIndex; } }

        public RectTransform closestItem
        {
            get
            {
                RectTransform child;
                GetChildAtSnapIndex(m_ClosestSnapPositionIndex, out child);
                return child;
            }
        }

        private List<RectTransform> m_ChildrenForSizeFromStartToEnd = new List<RectTransform>();
        public List<RectTransform> calculateChildren { get { return m_ChildrenForSizeFromStartToEnd; } }

        private List<RectTransform> m_ChildrenForSnappingFromStartToEnd = new List<RectTransform>();
        public List<RectTransform> snapChildren { get { return m_ChildrenForSnappingFromStartToEnd; } }


        private string filterWhitelistException = "The {0} is set to whitelist and is either empty or contains an empty object. You probably need to assign a child to the {0} or set the {0} to blacklist.";
        private string availableChildrenListEmptyException = "The Content has no children available for {0}. This is probably because they are all blacklisted. You should check what children you have blacklisted in your item filters and if you have Add Inactive Children checked.";

        private DrivenRectTransformTracker m_Tracker;
        private Scroller m_Scroller = new Scroller();

        private List<RectTransform> m_AvailableForCalculating = new List<RectTransform>();
        private List<RectTransform> m_AvailableForSnappingTo = new List<RectTransform>();

        private List<Vector2> m_SnapPositions = new List<Vector2>();
        
        private List<float> m_CalculateDistances = new List<float>();
        private List<float> m_SnapDistances = new List<float>();

        // The offset from handle position to mouse down position
        private Vector2 m_PointerStartLocalCursor = Vector2.zero;
        private Vector2 m_ContentStartPosition = Vector2.zero;

        private Bounds m_ContentBounds;
        private Bounds m_ViewBounds;

        private Vector2 m_PrevPosition = Vector2.zero;
        private Bounds m_PrevContentBounds;
        private Bounds m_PrevViewBounds;
        private RectTransform m_PrevClosestItem;
        private bool m_PrevScrolling;
        private float m_PrevTension;
        private float m_PrevFriction;
        private int m_PrevMinDuration;
        private int m_PrevMaxDuration;

        [NonSerialized]
        private bool m_HasRebuiltLayout = false;
        private bool m_UpdateContentSize;
        private bool m_UpdateSnapPositions;

        private Vector2 m_MinPos;
        private Vector2 m_MaxPos;
        private int m_ExtraLoopSpace;

        private bool m_WaitingForEndScrolling;
        private float m_TimeOfLastScroll;

        private Camera m_LastPressedCamera;

        [NonSerialized]
        private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                {
                    m_Rect = GetComponent<RectTransform>();
                }
                return m_Rect;
            }
        }

        private RectTransform m_ViewRect;
        protected RectTransform viewRect
        {
            get
            {
                if (m_ViewRect == null)
                {
                    m_ViewRect = m_Viewport;
                }
                if (m_ViewRect == null)
                {
                    m_ViewRect = (RectTransform)transform;
                }
                return m_ViewRect;
            }
        }

        private bool m_LayoutGroupWasEnabled;
        private LayoutGroup m_LayoutGroup;
        private bool contentIsLayoutGroup
        {
            get
            {
                if (m_Content == null)
                {
                    return false;
                }
                m_LayoutGroup = m_Content.GetComponent<LayoutGroup>();
                return m_LayoutGroup;
            }
        }
        
        private bool contentIsHorizonalLayoutGroup
        {
            get
            {
                if (m_Content == null)
                {
                    return false;
                }
                HorizontalLayoutGroup horizLayoutGroup = m_Content.GetComponent<HorizontalLayoutGroup>();
                return horizLayoutGroup && horizLayoutGroup.enabled;
            }
        }
        
        private bool contentIsVerticalLayoutGroup
        {
            get
            {
                if (m_Content == null)
                {
                    return false;
                }
                VerticalLayoutGroup vertLayoutGroup = content.GetComponent<VerticalLayoutGroup>();
                return vertLayoutGroup && vertLayoutGroup.enabled;
            }
        }
        
        private RectTransform firstCalculateChild
        {
            get
            {
                return m_ChildrenForSizeFromStartToEnd[0];
            }
        }
        
        private RectTransform lastCalculateChild
        {
            get
            {
                return m_ChildrenForSizeFromStartToEnd[m_ChildrenForSizeFromStartToEnd.Count - 1];
            }
        }

        private Vector3 contentTopLeftLocalViewRect
        {
            get
            {
                Vector3[] contentCorners = new Vector3[4];
                m_Content.GetWorldCorners(contentCorners);
                return viewRect.InverseTransformPoint(contentCorners[1]);
            }
        }

        private int axis
        {
            get
            {
                return (int)movementDirection;
            }
        }

        private int inverseAxis
        {
            get
            {
                return 1 - axis;
            }
        }

        private int movementDirectionMult
        {
            get
            {
                return (m_MovementDirection == MovementDirection.Vertical) ? -1 : 1;
            }
        }
        #endregion

        #region Temp
        #endregion

        #region SetupScrollSnap
        protected override void OnEnable()
        {
            base.OnEnable();
            
            if (contentIsLayoutGroup && m_LayoutGroup.enabled)
            {
                RebuildLayoutGroups();
            }
            if (contentIsLayoutGroup && Application.isPlaying)
            {
                m_LayoutGroupWasEnabled = m_LayoutGroup.enabled;
                m_LayoutGroup.enabled = false;
            }

            UpdatePrevData();
            UpdateLayout();
            JumpToSnappableChild(startItem);
            Loop(Direction.TowardsStart);
            Loop(Direction.TowardsEnd);

            if (m_HorizontalScrollbar && !m_Loop)
            {
                m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
                if (Application.isPlaying)
                {
                    m_HorizontalScrollbarEventsListener = m_HorizontalScrollbar.gameObject.AddComponent<ScrollBarEventsListener>();
                    m_HorizontalScrollbarEventsListener.onPointerDown += ScrollBarPointerDown;
                    m_HorizontalScrollbarEventsListener.onPointerUp += ScrollBarPointerUp;
                }
            }
            if (m_VerticalScrollbar && !m_Loop)
            {
                m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
                if (Application.isPlaying)
                {
                    m_VerticalScrollBarEventsListener = m_VerticalScrollbar.gameObject.AddComponent<ScrollBarEventsListener>();
                    m_VerticalScrollBarEventsListener.onPointerDown += ScrollBarPointerDown;
                    m_VerticalScrollBarEventsListener.onPointerUp += ScrollBarPointerUp;
                }
            }

            if (m_BackButton)
            {
                m_BackButton.onClick.AddListener(OnBack);
            }
            if (m_ForwardButton)
            {
                m_ForwardButton.onClick.AddListener(OnForward);
            }

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (m_HorizontalScrollbar)
            {
                m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
                if (m_HorizontalScrollbarEventsListener != null)
                {
                    m_HorizontalScrollbarEventsListener.onPointerDown -= ScrollBarPointerDown;
                    m_HorizontalScrollbarEventsListener.onPointerUp -= ScrollBarPointerUp;
                    DestroyImmediate(m_HorizontalScrollbarEventsListener);
                }
            }
            if (m_VerticalScrollbar)
            {
                m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
                if (m_VerticalScrollBarEventsListener != null)
                {
                    m_VerticalScrollBarEventsListener.onPointerDown -= ScrollBarPointerDown;
                    m_VerticalScrollBarEventsListener.onPointerUp -= ScrollBarPointerUp;
                    DestroyImmediate(m_VerticalScrollBarEventsListener);
                }
            }
            if (m_BackButton)
            {
                m_BackButton.onClick.RemoveListener(OnBack);
            }
            if (m_ForwardButton)
            {
                m_ForwardButton.onClick.RemoveListener(OnForward);
            }

            if (contentIsLayoutGroup && m_LayoutGroupWasEnabled)
            {
                m_LayoutGroup.enabled = true;
            }

            m_HasRebuiltLayout = false;
            m_Tracker.Clear();
            m_Velocity = Vector2.zero;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }
        
        /// <summary>
        /// Updates the Scroll Snap. Use only if you need updated info about the Scroll Snap before it has updated itself (e.g. first frame).
        /// If you need to modify the Scroll Snap (e.g. adding items, manipulating spacing, changing an item's snappability, removing items) please use the provided functions.
        /// </summary>
        public void UpdateLayout()
        {
            if (m_Content == null || m_Content.childCount == 0)
            {
                return;
            }
            Validate();
            EnsureLayoutHasRebuilt();
            GetValidChildren();
            SetupDrivenTransforms();
            GetChildrenFromStartToEnd();
            GetCalculateDistances();
            
            SetReferencePos(firstCalculateChild);

            ResizeContent();
            GetSnapPositions();
            GetSnapDistances();

            ResetContentPos();
        }

        private void SetupDrivenTransforms()
        {
            Vector2 anchorPos = new Vector2(0, 1);

            m_Tracker.Clear();

            m_Tracker.Add(this, m_Content, DrivenTransformProperties.Anchors);

            m_Content.anchorMax = anchorPos;
            m_Content.anchorMin = anchorPos;

            //So that we can calculate everything correctly
            foreach (RectTransform transform in m_AvailableForCalculating)
            {
                m_Tracker.Add(this, transform, DrivenTransformProperties.Anchors);

                transform.anchorMax = anchorPos;
                transform.anchorMin = anchorPos;
            }
        }

        private void GetValidChildren()
        {
            m_AvailableForCalculating.Clear();
            m_AvailableForSnappingTo.Clear();

            Func<RectTransform, bool> childIsAvailableForCalculating;
            Func<RectTransform, bool> childIsAvailableForSnappingTo;

            if (m_FilterModeForCalculatingSize == FilterMode.WhiteList)
            {
                if (m_CalculatingFilter.Count < 1 || m_CalculatingFilter.Contains(null))
                {
                    throw (new UnassignedReferenceException(string.Format(filterWhitelistException, "Calculate Size Filter")));
                }
                childIsAvailableForCalculating = (RectTransform child) => m_CalculatingFilter.Contains(child) || (m_AddInactiveChildrenToCalculatingFilter && !child.gameObject.activeInHierarchy);
            }
            else
            {
                childIsAvailableForCalculating = (RectTransform child) => !m_CalculatingFilter.Contains(child) && (!m_AddInactiveChildrenToCalculatingFilter || child.gameObject.activeInHierarchy);
            }


            if (m_FilterModeForSnapPositions == FilterMode.WhiteList)
            {
                if (m_SnapPositionsFilter.Count < 1 || m_SnapPositionsFilter.Contains(null))
                {
                    throw (new UnassignedReferenceException(string.Format(filterWhitelistException, "Available Snaps Filter")));
                }
                childIsAvailableForSnappingTo = (RectTransform child) => m_SnapPositionsFilter.Contains(child) || (m_AddInactiveChildrenToSnapPositionsFilter && !child.gameObject.activeInHierarchy);
            }
            else
            {
                childIsAvailableForSnappingTo = (RectTransform child) => !m_SnapPositionsFilter.Contains(child) && (!m_AddInactiveChildrenToSnapPositionsFilter || child.gameObject.activeInHierarchy);
            }

            foreach (RectTransform child in m_Content)
            {

                if (childIsAvailableForCalculating(child))
                {
                    m_AvailableForCalculating.Add(child);
                    if (childIsAvailableForSnappingTo(child))
                    {
                        m_AvailableForSnappingTo.Add(child);
                    }
                }
            }

            if (m_AvailableForCalculating.Count < 1)
            {
                throw (new MissingReferenceException(string.Format(availableChildrenListEmptyException, "calculating")));
            }

            if (m_AvailableForSnappingTo.Count < 1)
            {
                throw (new MissingReferenceException(string.Format(availableChildrenListEmptyException, "snapping to")));
            }
        }

        private void GetChildrenFromStartToEnd()
        {
            m_ChildrenForSizeFromStartToEnd.Clear();
            m_ChildrenForSnappingFromStartToEnd.Clear();
            foreach (RectTransform child in m_Content)
            {
                if (m_AvailableForCalculating.Contains(child))
                {
                    int insert = m_ChildrenForSizeFromStartToEnd.Count;
                    for (int i = 0; i < m_ChildrenForSizeFromStartToEnd.Count; i++)
                    {
                        if (TransformAIsNearerStart(child, m_ChildrenForSizeFromStartToEnd[i]))
                        {
                            insert = i;
                            break;
                        }
                    }
                    m_ChildrenForSizeFromStartToEnd.Insert(insert, child);
                }
            }

            foreach (RectTransform child in m_ChildrenForSizeFromStartToEnd)
            {
                if (m_AvailableForSnappingTo.Contains(child))
                {
                    m_ChildrenForSnappingFromStartToEnd.Add(child);
                }
            }
        } 

        private void GetCalculateDistances()
        {
            if (m_CalculateDistances.Count == 0)
            {
                for (int i = 0; i < m_ChildrenForSizeFromStartToEnd.Count - 1; i++)
                {
                    m_CalculateDistances.Add(DistanceOnAxis(m_ChildrenForSizeFromStartToEnd[i].anchoredPosition, m_ChildrenForSizeFromStartToEnd[i + 1].anchoredPosition, axis));
                }

                m_CalculateDistances.Add(GetCalculateDistance(m_ChildrenForSizeFromStartToEnd.Count - 1, m_EndSpacing, m_ChildrenForSizeFromStartToEnd.Count));
            }
        }

        private void ResizeContent()
        {
            UpdateBounds();
            m_ExtraLoopSpace = m_Loop ? (int)(Mathf.Max(m_CalculateDistances.ToArray()) / 2) : 0;
            float halfViewRect = movementDirectionMult * m_ViewBounds.extents[axis];
            int startOffset = (int)(halfViewRect - firstCalculateChild.anchoredPosition[axis]) + (movementDirectionMult * m_ExtraLoopSpace);
            int endOffset = (int)(halfViewRect - ((movementDirectionMult * m_Content.sizeDelta[axis]) - lastCalculateChild.anchoredPosition[axis])) + (movementDirectionMult * m_ExtraLoopSpace);

            if (!Application.isPlaying && contentIsLayoutGroup && m_LayoutGroup.enabled)
            {
                if (m_MovementDirection == MovementDirection.Horizontal)
                {
                    m_LayoutGroup.padding.left = m_LayoutGroup.padding.left + startOffset;
                    m_LayoutGroup.padding.right = m_LayoutGroup.padding.right + endOffset;
                    m_Content.sizeDelta = new Vector2(m_Content.sizeDelta.x + startOffset + endOffset, m_Content.sizeDelta.y);
                }
                else
                {
                    m_LayoutGroup.padding.top = m_LayoutGroup.padding.top - startOffset;
                    m_LayoutGroup.padding.bottom = m_LayoutGroup.padding.bottom - endOffset;
                    m_Content.sizeDelta = new Vector2(m_Content.sizeDelta.x, m_Content.sizeDelta.y - startOffset - endOffset);
                }
                RebuildLayoutGroups();
            }
            else
            {
                foreach (RectTransform child in m_ChildrenForSizeFromStartToEnd)
                {
                    Vector3 newAnchorPos = child.anchoredPosition;
                    newAnchorPos[axis] = newAnchorPos[axis] + startOffset;
                    child.anchoredPosition = newAnchorPos;
                }
                float totalSize = Mathf.Abs(lastCalculateChild.anchoredPosition[axis]) + Mathf.Abs(halfViewRect) + m_ExtraLoopSpace;
                m_Content.sizeDelta = (movementDirection == MovementDirection.Horizontal) ? new Vector2(totalSize, m_Content.sizeDelta.y) : new Vector2(m_Content.sizeDelta.x, totalSize);
            }
            SetNormalizedPosition(1, 0);
            SetNormalizedPosition(0, 1);
            m_MinPos = m_Content.anchoredPosition;
            SetNormalizedPosition(0, 0);
            SetNormalizedPosition(1, 1);
            m_MaxPos = m_Content.anchoredPosition;
        }

        private void GetSnapPositions()
        {
            m_SnapPositions.Clear();

            foreach(RectTransform child in m_ChildrenForSnappingFromStartToEnd)
            {
                float normalizedPosition;
                GetNormalizedPositionOfChild(child, out normalizedPosition);
                SetNormalizedPosition(normalizedPosition, axis);
                m_SnapPositions.Add(m_Content.anchoredPosition);
            }
        }

        private void GetSnapDistances()
        {
            if (m_SnapDistances.Count == 0)
            {
                float currentDistance = 0;
                int index = 0;
                GetCalculateIndexOfChild(m_ChildrenForSnappingFromStartToEnd[0], out index);

                for (int i = 0; i < m_CalculateDistances.Count; i++)
                {
                    currentDistance += m_CalculateDistances[LoopIndex(index, m_CalculateDistances.Count)];

                    if (m_ChildrenForSnappingFromStartToEnd.Contains(m_ChildrenForSizeFromStartToEnd[LoopIndex(index + 1, m_ChildrenForSizeFromStartToEnd.Count)]))
                    {
                        m_SnapDistances.Add(currentDistance);
                        currentDistance = 0;
                    }

                    index++;
                }
            }
        }
        #endregion

        #region Scrolling
        private void LateUpdate()
        {
            if (!m_Content)
            {
                return;
            }

            EnsureLayoutHasRebuilt();
            UpdateBounds();
            CheckLayoutUpdates();
            bool loop = LoopBasedOnVelocity();
            float deltaTime = Time.unscaledDeltaTime;
            Vector2 offset = CalculateOffset(Vector2.zero);

            if (m_WaitingForEndScrolling && Time.time - m_TimeOfLastScroll > m_ScrollDelay)
            {
                m_WaitingForEndScrolling = false;
                SelectSnapPos(StartMovementEventType.OnScroll);
            }

            if (m_Scroller.ComputeScrollOffset())
            {
                m_Content.anchoredPosition = m_Scroller.currentPosition;
            }

            if (!loop)
            {
                Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
                m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
            }

            m_ClosestSnapPositionIndex = m_SnapPositions.IndexOf(FindClosestSnapPositionToPosition(m_Content.anchoredPosition));

            if (m_Content.anchoredPosition != m_PrevPosition)
            {
                m_OnValueChanged.Invoke(new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition));
            }
            if (closestItem != m_PrevClosestItem)
            {
                m_ClosestSnapPositionChanged.Invoke(m_ClosestSnapPositionIndex);
            }
            if (m_Scroller.isFinished && m_PrevScrolling)
            {
                m_SnappedToItem.Invoke(m_ClosestSnapPositionIndex);
                m_Velocity = Vector2.zero;
            }


            if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || m_Content.anchoredPosition != m_PrevPosition)
            {
                UpdateScrollbars(offset);
            }
            UpdatePrevData();
        }

        private void ScrollBarPointerDown(PointerEventData ped)
        {
            m_StartMovementEvent.Invoke(StartMovementEventType.ScrollBar);
            m_Scroller.ForceFinish();
        }

        private void ScrollBarPointerUp(PointerEventData ped)
        {
            SelectSnapPos(StartMovementEventType.ScrollBar);
        }
        
        private void OnBack()
        {
            if (!m_ButtonAlwaysGoToEnd && !m_Loop && closestSnapPositionIndex <= m_ButtonItemsToMoveBy - 1)
            {
                return;
            }

            if (!m_Loop && closestSnapPositionIndex == 0)
            {
                return;
            }

            int itemsToMoveBy = Mathf.Min(closestSnapPositionIndex, m_ButtonItemsToMoveBy);

            m_StartMovementEvent.Invoke(StartMovementEventType.ButtonPress);
            m_Scroller.ForceFinish();

            Vector2 targetPosition = m_Content.anchoredPosition;
            targetPosition[axis] = m_SnapPositions[closestSnapPositionIndex][axis] + (movementDirectionMult * GetSnapDistance(closestSnapPositionIndex - itemsToMoveBy, closestSnapPositionIndex));

            Interpolator interpolator = GetInterpolator(m_ButtonInterpolator, m_ButtonTension);
            ImplimentCustomInterpolator(StartMovementEventType.ButtonPress, ref interpolator);

            m_Scroller.StartScroll(m_Content.anchoredPosition, targetPosition, m_ButtonAnimationDuration, interpolator);

            m_TargetItemSelected.Invoke(closestSnapPositionIndex - 1);
        }

        private void OnForward()
        {
            if (!m_ButtonAlwaysGoToEnd && !m_Loop && closestSnapPositionIndex >= m_SnapPositions.Count - m_ButtonItemsToMoveBy)
            {
                return;
            }

            if (!m_Loop && closestSnapPositionIndex == m_SnapPositions.Count - 1)
            {
                return;
            }

            int itemsToMoveBy = Mathf.Min(m_SnapPositions.Count - (closestSnapPositionIndex + 1), m_ButtonItemsToMoveBy);

            m_StartMovementEvent.Invoke(StartMovementEventType.ButtonPress);
            m_Scroller.ForceFinish();

            Vector2 targetPosition = m_Content.anchoredPosition;
            targetPosition[axis] = m_SnapPositions[closestSnapPositionIndex][axis] - (movementDirectionMult * GetSnapDistance(closestSnapPositionIndex, closestSnapPositionIndex + itemsToMoveBy));

            Interpolator interpolator = GetInterpolator(m_ButtonInterpolator, m_ButtonTension);
            ImplimentCustomInterpolator(StartMovementEventType.ButtonPress, ref interpolator);

            m_Scroller.StartScroll(m_Content.anchoredPosition, targetPosition, m_ButtonAnimationDuration, interpolator);

            m_TargetItemSelected.Invoke(closestSnapPositionIndex + 1);
        }

        public virtual void OnScroll(PointerEventData data)
        {
            if (!IsActive())
            {
                return;
            }
            m_TimeOfLastScroll = Time.time;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector2 delta = data.scrollDelta;
            delta.y *= -1;
            if (Mathf.Abs(delta[inverseAxis]) > Mathf.Abs(delta[axis]))
            {
                delta[axis] = delta[inverseAxis];
            }
            delta[inverseAxis] = 0;

            if (!m_WaitingForEndScrolling)
            {
                m_StartMovementEvent.Invoke(StartMovementEventType.OnScroll);
                m_WaitingForEndScrolling = true;
                if (Mathf.Sign(delta[axis]) != Mathf.Sign(m_Velocity[axis]))
                {
                    m_Scroller.AbortAnimation();
                }
            }

            Vector2 position = m_Content.anchoredPosition;
            position += delta * m_ScrollSensitivity;
            if (m_MovementType == MovementType.Clamped)
            {
                position += CalculateOffset(position - m_Content.anchoredPosition);
            }

            SetContentAnchoredPosition(position);
            UpdateBounds();
        }

        public virtual void OnBeginDrag(PointerEventData ped)
        {
            if (!IsActive() || !m_AllowTouchInput)
            {
                return;
            }
            m_LastPressedCamera = ped.pressEventCamera;

            m_StartMovementEvent.Invoke(StartMovementEventType.Touch);

            UpdateBounds();

            m_PointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, ped.position, ped.pressEventCamera, out m_PointerStartLocalCursor);
            m_ContentStartPosition = m_Content.anchoredPosition;

            m_Scroller.ForceFinish();
            m_Velocity = Vector2.zero;
        }
        
        public virtual void OnDrag(PointerEventData ped)
        {
            if (!IsActive() || !m_AllowTouchInput)
            {
                return;
            }

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, ped.position, ped.pressEventCamera, out localCursor))
            {
                return;
            }

            UpdateBounds();

            var pointerDelta = localCursor - m_PointerStartLocalCursor;
            Vector2 localtemp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, Input.mousePosition, Camera.main, out localtemp);
            Vector2 position = m_ContentStartPosition + pointerDelta;
            
            Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
            position += offset;

            if (m_MovementType == MovementType.Elastic)
            {
                if (offset.x != 0)
                {
                    position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x);
                }
                if (offset.y != 0)
                {
                    position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y);
                }
            }

            SetContentAnchoredPosition(position);
        }

        public virtual void OnEndDrag(PointerEventData ped)
        {
            if (!IsActive() || !m_AllowTouchInput)
            {
                return;
            }

            SelectSnapPos(StartMovementEventType.Touch);
        }

        private void SelectSnapPos(StartMovementEventType eventType)
        {
            float decelerationRate = 1 - m_Friction;
            Interpolator interpolator = GetInterpolator(m_InterpolatorType, m_Tension);
            switch (eventType)
            {
                case StartMovementEventType.OnScroll:
                    decelerationRate = 1 - m_ScrollFriction;
                    interpolator = GetInterpolator(m_ScrollInterpolator, m_ScrollTension);
                    break;
                case StartMovementEventType.ScrollBar:
                    decelerationRate = 1 - m_ScrollBarFriction;
                    interpolator = GetInterpolator(m_ScrollBarInterpolator, m_ScrollBarTension);
                    break;
            }
            ImplimentCustomInterpolator(eventType, ref interpolator);

            Vector2 referencePos = m_Content.anchoredPosition;
            if (m_SimulateFlings)
            {
                referencePos.x += Mathf.Sign(m_Velocity.x) * m_Scroller.CalculateMovementDelta(Mathf.Abs(m_Velocity.x), decelerationRate);
                referencePos.y += Mathf.Sign(m_Velocity.y) * m_Scroller.CalculateMovementDelta(Mathf.Abs(m_Velocity.y), decelerationRate);
            }

            Vector2 snapPos = m_Content.anchoredPosition;
            switch (m_SnapType)
            {
                case SnapType.SnapToLastPassed:
                    snapPos[axis] = FindLastSnapPositionBeforePosition(referencePos, GetDirectionFromVelocity(m_Velocity, axis), m_Loop)[axis];
                    break;
                case SnapType.SnapToNearest:
                    snapPos[axis] = FindClosestSnapPositionToPosition(referencePos, GetDirectionFromVelocity(m_Velocity, axis), m_Loop)[axis];
                    break;
                case SnapType.SnapToNext:
                    snapPos[axis] = FindNextSnapAfterPosition(referencePos, GetDirectionFromVelocity(m_Velocity, axis), m_Loop)[axis];
                    break;
            }

            snapPos[inverseAxis] = Mathf.Clamp(snapPos[inverseAxis], m_MinPos[inverseAxis], m_MaxPos[inverseAxis]);
            if (!m_Loop)
            {
                snapPos[axis] = Mathf.Clamp(snapPos[axis], m_MinPos[axis], m_MaxPos[axis]);
            }
            
            float decelRate = m_Scroller.CalculateDecelerationRate(Mathf.Abs(m_Velocity[axis]), Mathf.Abs(snapPos[axis] - m_Content.anchoredPosition[axis]));
            float duration = m_Scroller.CalculateDuration(Mathf.Abs(m_Velocity[axis]), decelRate);
            duration = Mathf.Clamp(duration, m_MinDuration, m_MaxDuration);
            
            m_Scroller.StartScroll(m_Content.anchoredPosition, snapPos, duration, interpolator);

            m_TargetItemSelected.Invoke(GetSnapIndexOfSnapPosition(snapPos, GetDirectionFromVelocity(m_Velocity, axis)));
        }
        #endregion

        #region Looping
        private bool LoopBasedOnVelocity()
        {
            if (m_Velocity[axis] == 0)
            {
                return false;
            }

            return Loop(GetDirectionFromVelocity(m_Velocity, axis));
        }

        private bool Loop(Direction direction)
        {
            if (!m_Loop || !Application.isPlaying)
            {
                return false;
            }

            UpdateBounds();

            bool looped = false;
            float distance = (m_ContentBounds.size[axis] - m_ViewBounds.size[axis]) / 2f;
            Vector2 totalOffset = Vector2.zero;
            Vector2 contentStartSize = m_Content.sizeDelta;

            if (direction == Direction.TowardsStart)
            {
                m_ChildrenForSizeFromStartToEnd.Reverse();
            }

            for (int i = 0; i < m_ChildrenForSizeFromStartToEnd.Count; i++)
            {
                RectTransform child = m_ChildrenForSizeFromStartToEnd[i];
                Vector3 childLocation = viewRect.InverseTransformPoint(child.position);
                
                bool loopAtEnd = (direction == Direction.TowardsStart) && LoopAtEnd(childLocation, distance);
                bool loopAtStart = (direction == Direction.TowardsEnd) && LoopAtStart(childLocation, distance);

                if (loopAtEnd || loopAtStart)
                {
                    looped = true;

                    if (m_ChildrenForSizeFromStartToEnd.Count > 1)
                    {
                        SetReferencePos(m_ChildrenForSizeFromStartToEnd[LoopIndex(i + 1, m_ChildrenForSizeFromStartToEnd.Count)]);
                    }

                    if (loopAtEnd)
                    {
                        if (m_ChildrenForSizeFromStartToEnd.Count == 1)
                        {
                            Vector2 newContentPos = m_Content.anchoredPosition;
                            totalOffset[axis] = -m_CalculateDistances[0] * movementDirectionMult;
                            newContentPos[axis] = m_Content.anchoredPosition[axis] - (m_CalculateDistances[0] * movementDirectionMult);
                            m_Content.anchoredPosition = newContentPos;
                        }
                        else
                        {
                            float movementAmount = m_CalculateDistances[m_CalculateDistances.Count - 1];
                            totalOffset[axis] -= movementDirectionMult * movementAmount;

                            Vector3 newChildPos = m_ChildrenForSizeFromStartToEnd[LoopIndex(i - 1, m_ChildrenForSizeFromStartToEnd.Count)].anchoredPosition;
                            newChildPos[axis] -= movementDirectionMult * movementAmount;
                            child.anchoredPosition = newChildPos;

                            float cacheCalculateDistance = m_CalculateDistances[m_CalculateDistances.Count - 1];
                            m_CalculateDistances.RemoveAt(m_CalculateDistances.Count - 1);
                            m_CalculateDistances.Insert(0, cacheCalculateDistance);

                            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
                            {
                                float cacheSnapDistance = m_SnapDistances[m_SnapDistances.Count - 1];
                                m_SnapDistances.RemoveAt(m_SnapDistances.Count - 1);
                                m_SnapDistances.Insert(0, cacheSnapDistance);
                            }
                        }
                    }
                    else if (loopAtStart)
                    {
                        if (m_ChildrenForSizeFromStartToEnd.Count == 1)
                        {
                            Vector2 newContentPos = m_Content.anchoredPosition;
                            totalOffset[axis] = m_CalculateDistances[0] * movementDirectionMult;
                            newContentPos[axis] = m_Content.anchoredPosition[axis] + (m_CalculateDistances[0] * movementDirectionMult);
                            m_Content.anchoredPosition = newContentPos;
                        }
                        else
                        {
                            float movementAmount = m_CalculateDistances[m_CalculateDistances.Count - 1];
                            totalOffset[axis] += movementAmount * movementDirectionMult;

                            Vector3 newChildPos = child.anchoredPosition;
                            newChildPos[axis] = m_ChildrenForSizeFromStartToEnd[LoopIndex(i - 1, m_ChildrenForSizeFromStartToEnd.Count)].anchoredPosition[axis] + (movementAmount * movementDirectionMult);
                            child.anchoredPosition = newChildPos;

                            float cacheCalculateDistance = m_CalculateDistances[0];
                            m_CalculateDistances.RemoveAt(0);
                            m_CalculateDistances.Insert(m_CalculateDistances.Count, cacheCalculateDistance);

                            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
                            {
                                float cacheSnapDistance = m_SnapDistances[0];
                                m_SnapDistances.RemoveAt(0);
                                m_SnapDistances.Insert(m_SnapDistances.Count, cacheSnapDistance);
                            }
                        }
                    }

                    if (m_ChildrenForSizeFromStartToEnd.Count > 1)
                    {
                        totalOffset += ResetContentPos();
                    }
                }
            }

            if (direction == Direction.TowardsStart)
            {
                m_ChildrenForSizeFromStartToEnd.Reverse();
            }

            if (looped)
            {
                GetChildrenFromStartToEnd();
                SetReferencePos(firstCalculateChild);
                ResizeContent();
                GetSnapPositions();
                ResetContentPos();

                Vector2 localCursor;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, Input.mousePosition, m_LastPressedCamera, out localCursor);
                m_PointerStartLocalCursor[axis] = localCursor[axis];
                m_ContentStartPosition[axis] = m_Content.anchoredPosition[axis];


                if (!m_Scroller.isFinished)
                {
                    Vector2 sizeShift = ((contentStartSize - m_Content.sizeDelta) / 2);
                    if ((m_MovementDirection == MovementDirection.Horizontal && direction == Direction.TowardsEnd) || (m_MovementDirection == MovementDirection.Vertical && direction == Direction.TowardsStart))
                    {
                        sizeShift = -sizeShift;
                    }
                    m_Scroller.ShiftAnimation(totalOffset - sizeShift);
                }

            }

            return looped;
        }

        private bool LoopAtEnd(Vector3 childLocation, float distance)
        {
            if (m_MovementDirection == MovementDirection.Horizontal)
            {
                return childLocation[axis] >= distance;
            }
            else
            {
                return childLocation[axis] <= -distance;
            }
        }

        private bool LoopAtStart(Vector3 childLocation, float distance)
        {
            if (m_MovementDirection == MovementDirection.Horizontal)
            {
                return childLocation[axis] <= -distance;
            }
            else
            {
                return childLocation[axis] >= distance;
            }
        }

        private void ShiftChildrenForEndSpacing(int calculateIndex, float spacing) //does not manipulate calculateDistances list
        {
            if (calculateIndex + 1 < m_ChildrenForSizeFromStartToEnd.Count)
            {
                RectTransform child = m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndex, m_ChildrenForSizeFromStartToEnd.Count)];
                RectTransform childAfterChild = m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndex + 1, m_ChildrenForSizeFromStartToEnd.Count)];

                float movementAmount = (child.anchoredPosition[axis] + (movementDirectionMult * GetCalculateDistance(calculateIndex, spacing, calculateIndex + 1))) - childAfterChild.anchoredPosition[axis];
                for (int i = calculateIndex + 1; i < m_ChildrenForSizeFromStartToEnd.Count; i++)
                {
                    Vector2 newSiblingPos = m_ChildrenForSizeFromStartToEnd[i].anchoredPosition;
                    newSiblingPos[axis] = newSiblingPos[axis] + movementAmount;
                    m_ChildrenForSizeFromStartToEnd[i].anchoredPosition = newSiblingPos;
                }
            }
        }

        private void SetParentToContent(RectTransform child)
        {
            child.SetParent(m_Content, false);
            m_Tracker.Add(this, child, DrivenTransformProperties.Anchors);
            child.anchorMax = new Vector2(0, 1);
            child.anchorMin = new Vector2(0, 1);
        }

        private void SetParentToNewParent(RectTransform child, RectTransform newParent)
        {
            if (newParent == null)
            {
                child.SetParent(GetCanvasTransform(child.parent));
            }
            else
            {
                child.SetParent(newParent);
            }
        }

        private RectTransform GetTrackingChild(int calculateIndex, LockMode lockMode)
        {
            if (lockMode == LockMode.Before)
            {
                return m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndex - 1, m_ChildrenForSizeFromStartToEnd.Count)];
            }
            else
            {
                return m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndex + 1, m_ChildrenForSizeFromStartToEnd.Count)];
            }
        }

        private List<float> CombineDistance(int middleIndex, List<float> list)
        {
            float combinedDistance = list[LoopIndex(middleIndex - 1, list.Count)] + list[LoopIndex(middleIndex, list.Count)];
            list[LoopIndex(middleIndex - 1, list.Count)] = combinedDistance;
            list.RemoveAt(middleIndex);
            return list;
        }

        private void RemoveViaCombine(RectTransform child)
        {
            int calculateIndexOfChild = 0;
            GetCalculateIndexOfChild(child, out calculateIndexOfChild);
            CombineDistance(calculateIndexOfChild, m_CalculateDistances);

            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
            {
                int snapIndexOfChild = 0;
                GetSnapIndexOfChild(child, out snapIndexOfChild);
                CombineDistance(snapIndexOfChild, m_SnapDistances);
                m_UpdateSnapPositions = true;
            }

            m_SnapPositionsFilter.Remove(child);
            m_CalculatingFilter.Remove(child);
            m_ChildrenForSizeFromStartToEnd.Remove(child);
            m_ChildrenForSnappingFromStartToEnd.Remove(child);
        }

        private void RemoveViaShift(RectTransform child, float calculateSpacing, LockMode lockMode)
        {
            calculateSpacing = Mathf.Max(0, calculateSpacing);

            int calculateIndexOfChild = 0;
            GetCalculateIndexOfChild(child, out calculateIndexOfChild);

            int snapIndexOfChild = 0;
            GetSnapIndexOfChild(child, out snapIndexOfChild);

            SetReferencePos(GetTrackingChild(calculateIndexOfChild, lockMode));

            RectTransform calculateBeforeChild = m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndexOfChild - 1, m_ChildrenForSizeFromStartToEnd.Count)];
            RectTransform calculateAfterChild = m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndexOfChild + 1, m_ChildrenForSizeFromStartToEnd.Count)];

            m_CalculateDistances[LoopIndex(calculateIndexOfChild - 1, m_ChildrenForSizeFromStartToEnd.Count)] = GetCalculateDistance(calculateIndexOfChild - 1, calculateSpacing, calculateIndexOfChild + 1);
            m_CalculateDistances.RemoveAt(calculateIndexOfChild);

            m_ChildrenForSizeFromStartToEnd.Remove(child);
            ShiftChildrenForEndSpacing(calculateIndexOfChild - 1, calculateSpacing);

            m_SnapDistances[LoopIndex(snapIndexOfChild - 1, m_SnapDistances.Count)] = GetSnapDistance(snapIndexOfChild - 1, snapIndexOfChild + 1);
            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
            {
                m_SnapDistances.RemoveAt(snapIndexOfChild);
            }

            m_SnapPositionsFilter.Remove(child);
            m_CalculatingFilter.Remove(child);
            m_ChildrenForSnappingFromStartToEnd.Remove(child);
            m_UpdateContentSize = true;
            m_UpdateSnapPositions = true;
        }

        private void InsertChild(RectTransform child, int calculateIndex, float nonAxisPosition, float startSpacing, float endSpacing, RectTransform trackingChild, bool snappable)
        {
            SetReferencePos(trackingChild);

            startSpacing = Mathf.Max(0, startSpacing);
            endSpacing = Mathf.Max(0, endSpacing);
            RectTransform calculateBeforeChild = m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndex - 1, m_ChildrenForSizeFromStartToEnd.Count)];
            RectTransform calculateAfterChild = m_ChildrenForSizeFromStartToEnd[LoopIndex(calculateIndex + 1, m_ChildrenForSizeFromStartToEnd.Count)];

            Vector2 newChildPos = Vector2.zero;
            newChildPos[inverseAxis] = nonAxisPosition;
            if (calculateIndex > 0)
            {
                newChildPos[axis] = calculateBeforeChild.anchoredPosition[axis] + (movementDirectionMult * GetCalculateDistance(calculateIndex - 1, startSpacing, calculateIndex));
            }
            child.anchoredPosition = newChildPos;
            
            ShiftChildrenForEndSpacing(calculateIndex, endSpacing);

            int snapIndexOfChild = 0;
            GetSnapIndexOfChild(child, out snapIndexOfChild);
            m_ChildrenForSnappingFromStartToEnd.Insert(snapIndexOfChild, child);
            
            if (m_CalculateDistances.Count == 0)
            {
                m_CalculateDistances.Add(GetCalculateDistance(calculateIndex - 1, startSpacing + endSpacing, calculateIndex));
            }
            else
            {
                m_CalculateDistances[LoopIndex(calculateIndex - 1, m_CalculateDistances.Count)] = GetCalculateDistance(calculateIndex - 1, startSpacing, calculateIndex);
                m_CalculateDistances.Insert(calculateIndex, GetCalculateDistance(calculateIndex, endSpacing, calculateIndex + 1));
            }

            if (snappable)
            {
                if (m_SnapDistances.Count == 0)
                {
                    m_SnapDistances.Add(GetSnapDistance(snapIndexOfChild - 1, snapIndexOfChild));
                }
                else
                {
                    m_SnapDistances[LoopIndex(snapIndexOfChild - 1, m_SnapDistances.Count)] = GetSnapDistance(snapIndexOfChild - 1, snapIndexOfChild);
                    m_SnapDistances.Insert(snapIndexOfChild, GetSnapDistance(snapIndexOfChild, snapIndexOfChild + 1));
                }

                if (m_FilterModeForSnapPositions == FilterMode.WhiteList)
                {
                    m_SnapPositionsFilter.Add(child);
                }
                m_AvailableForSnappingTo.Add(child);
            }
            else
            {
                if (m_SnapDistances.Count > 0)
                {
                    m_SnapDistances[LoopIndex(snapIndexOfChild - 1, m_SnapDistances.Count)] = GetSnapDistance(snapIndexOfChild - 1, snapIndexOfChild) + GetSnapDistance(snapIndexOfChild, snapIndexOfChild + 1);
                }
                if (m_FilterModeForSnapPositions == FilterMode.BlackList)
                {
                    m_SnapPositionsFilter.Add(child);
                }
                m_ChildrenForSnappingFromStartToEnd.Remove(child);
            }
            
            if (m_FilterModeForCalculatingSize == FilterMode.WhiteList)
            {
                m_CalculatingFilter.Add(child);
            }
            m_AvailableForCalculating.Add(child);
            
            m_UpdateContentSize = true;
            m_UpdateSnapPositions = true;

            ResetContentPos();
        }
        #endregion

        #region Public Functions

        /// <summary>
        /// Gets the normalized position of the scroll snap when it is snapped to the child. 
        /// </summary>
        /// <returns>Returns true if the supplied RectTransform is a child of the content.</returns>
        public bool GetNormalizedPositionOfChild(RectTransform child, out float normalizedPosition)
        {
            UpdateBounds();
            Vector2 startPos = new Vector2(m_ViewBounds.extents.x, -m_ViewBounds.extents.y); //if we think of this as relative to the top left corner, half of the view on the x and half of the view on the y gives us the middle of the view
            normalizedPosition = DistanceOnAxis(child.anchoredPosition, startPos, axis) / (m_ContentBounds.size[axis] - m_ViewBounds.size[axis]);
            return child.parent == m_Content;
        }

        /// <summary>
        /// Gets the position of the content in the content's local coordinates when it is snapped to the child. 
        /// </summary>
        /// <returns>Returns true if the supplied RectTransform is a child of the content.</returns>
        public bool GetPositionOfChild(RectTransform child, out Vector2 position)
        {
            Vector2 anchoredPos = m_Content.anchoredPosition;
            float normalizedPos;
            GetNormalizedPositionOfChild(child, out normalizedPos);
            SetNormalizedPosition(normalizedPos, axis);
            position = m_Content.anchoredPosition;
            m_Content.anchoredPosition = anchoredPos;
            return child.parent == m_Content;
        }

        /// <summary>
        /// Gets the index of the child, based on the snappable items. 
        /// </summary>
        /// <returns>Returns true if the the supplied RectTransform is a valid snap position.</returns>
        public bool GetSnapIndexOfChild(RectTransform child, out int index)
        {
            index = 0;
            foreach (RectTransform rect in m_ChildrenForSnappingFromStartToEnd)
            {
                if(TransformAIsNearerStart(rect, child))
                {
                    index++;
                }
                else
                {
                    break;
                }
            }

            return m_ChildrenForSnappingFromStartToEnd.Contains(child);
        }

        /// <summary>
        /// Gets the index of the child, based on the calculable items.
        /// </summary>
        /// <returns>Returns true if the supplied RectTransform is a calculable item.</returns>
        public bool GetCalculateIndexOfChild(RectTransform child, out int index)
        {
            index = 0;
            foreach (RectTransform rect in m_ChildrenForSizeFromStartToEnd)
            {
                if (TransformAIsNearerStart(rect, child))
                {
                    index++;
                }
                else
                {
                    break;
                }
            }

            return (m_ChildrenForSizeFromStartToEnd.Contains(child));
        }

        /// <summary>
        /// Gets the normalized position of the content when snapped to the supplied snap position. 
        /// </summary>
        /// <returns>Returns true if the supplied snap position index is a valid snap position.</returns>
        public bool GetNormalizedPositionOfSnapPosition(int snapPositionIndex, out float normalizedPosition)
        {
            if (snapPositionIndex >= 0 && snapPositionIndex < m_SnapPositions.Count)
            {
                RectTransform child;
                GetChildAtSnapIndex(snapPositionIndex, out child);
                GetNormalizedPositionOfChild(child, out normalizedPosition);
                return true;
            }
            normalizedPosition = 0f;
            return false;
        }

        /// <summary>
        /// Gets the position of the content in the content's local coordinates when snapped to the snap position at the specified coordinates. 
        /// </summary>
        /// <returns>Returns true if the supplied snap position index is a valid snap position.</returns>
        public bool GetSnapPositionAtIndex(int snapPositionIndex, out Vector2 location)
        {
            if (snapPositionIndex >= 0 && snapPositionIndex < m_SnapPositions.Count)
            {
                location = m_SnapPositions[snapPositionIndex];
                return true;
            }
            location = Vector2.zero;
            return false;
        }

        /// <summary>
        /// Gets the item at the supplied snap index.
        /// </summary>
        /// <returns>Returns true if the supplied snap position index is a valid snap position.</returns>
        public bool GetChildAtSnapIndex(int snapPositionIndex, out RectTransform child)
        {
            if (snapPositionIndex >= 0 && snapPositionIndex < m_SnapPositions.Count)
            {
                child = m_ChildrenForSnappingFromStartToEnd[snapPositionIndex];
                return true;
            }
            child = null;
            return false;
        }

        /// <summary>
        /// Gets the item at the supplied calculate index.
        /// </summary>
        /// <returns>Returns true if the supplied calculate index is a valid calculable child.</returns>
        public bool GetChildAtCalculateIndex(int calculateIndex, out RectTransform child)
        {
            if(calculateIndex >= 0 && calculateIndex < m_ChildrenForSizeFromStartToEnd.Count)
            {
                child = m_ChildrenForSizeFromStartToEnd[calculateIndex];
                return true;
            }
            child = null;
            return false;
        }

        /// <summary>
        /// Insert a new child into the Scroll Snap.
        /// </summary>
        /// <param name="child">The RectTransform to be inserted.</param>
        /// <param name="calculateIndex">What index you want the child to be inserted at, based on all the calculable items.</param>
        /// <param name="nonAxisPosition">The position of the child on the non-scrolling axis relative to the top left of the content.</param>
        /// <param name="startSpacing">The spacing between the end (bottom/right) edge of the previous calculable child, and the start (top/left) edge of the child being added.</param>
        /// <param name="endSpacing">The spacing between the start (top/left) edge of the next calculable child, and the end (bottom/right) edge of the child being added.</param>
        /// <param name="lockMode">Determines which calculable items will be "shifted" and which will be "locked" in the same position, relative to the center of the Scroll Snap.</param>
        /// <param name="snappable">If the new child should be snappable.</param>
        public void InsertChild(RectTransform child, int calculateIndex, float nonAxisPosition, float startSpacing, float endSpacing, LockMode lockMode, bool snappable)
        {
            if (child == null)
            {
                return;
            }

            calculateIndex = Mathf.Clamp(calculateIndex, 0, m_ChildrenForSizeFromStartToEnd.Count);
            if (m_MovementDirection == MovementDirection.Horizontal)
            {
                nonAxisPosition = Mathf.Min(0, nonAxisPosition);
            }
            else
            {
                nonAxisPosition = Mathf.Max(0, nonAxisPosition);
            }

            SetParentToContent(child);
            m_ChildrenForSizeFromStartToEnd.Insert(calculateIndex, child);

            InsertChild(child, calculateIndex, nonAxisPosition, startSpacing, endSpacing, GetTrackingChild(calculateIndex, lockMode), snappable);
        }

        /// <summary>
        /// Insert a new child into the Scroll Snap.
        /// </summary>
        /// <param name="child">The RectTransform to be inserted.</param>
        /// <param name="worldPos">The position, in world coordinates, the new child will be placed at.</param>
        /// <param name="startSpacing">The spacing between the end (bottom/right) edge of the previous calculable child, and the start (top/left) edge of the child being added.</param>
        /// <param name="endSpacing">The spacing between the start (top/left) edge of the next calculable child, and the end (bottom/right) edge of the child being added.</param>
        /// <param name="snappable">If the new child should be snappable</param>
        public void InsertChild(RectTransform child, Vector3 worldPos, float startSpacing, float endSpacing, bool snappable)
        {
            if (child == null)
            {
                return;
            }

            Matrix4x4 matrix = Matrix4x4.TRS(m_ContentWorldCorners[1], m_Content.rotation, m_Content.localScale);
            Vector2 posRelativeToContentTopLeft = matrix.inverse.MultiplyPoint3x4(worldPos);

            SetParentToContent(child);
            child.anchoredPosition = posRelativeToContentTopLeft;

            int calculateIndexOfChild = 0;
            GetCalculateIndexOfChild(child, out calculateIndexOfChild);
            m_ChildrenForSizeFromStartToEnd.Insert(calculateIndexOfChild, child);

            InsertChild(child, calculateIndexOfChild, posRelativeToContentTopLeft[inverseAxis], startSpacing, endSpacing, child, snappable);
        }

        /// <summary>
        /// Sets the content's position to be aligned with the Snap Position at the snapIndex. Does not animate just "jumps".
        /// </summary>
        /// <param name="snapIndex">The index of the Snap Position you would like to align the content with.</param>
        /// <returns>Returns true if the snapIndex is a valid index</returns>
        public bool JumpToSnapIndex(int snapIndex)
        {
            Vector2 contentPosition;
            if (GetSnapPositionAtIndex(snapIndex, out contentPosition))
            {
                contentPosition[inverseAxis] = m_Content.anchoredPosition[inverseAxis];
                m_Content.anchoredPosition = contentPosition;
                Loop(Direction.TowardsStart);
                Loop(Direction.TowardsEnd);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the content's position to be aligned with the Snap Position of the child. Does not animate just "jumps".
        /// </summary>
        /// <param name="child">The child you would like to align the content with.</param>
        /// <returns>Returns true if the child is a valid (snappable) child.</returns>
        public bool JumpToSnappableChild(RectTransform child)
        {
            if (child == null)
            {
                return false;
            }

            Vector2 contentPosition;
            GetPositionOfChild(child, out contentPosition);
            if (m_AvailableForSnappingTo.Contains(child))
            {
                contentPosition[inverseAxis] = m_Content.anchoredPosition[inverseAxis];
                m_Content.anchoredPosition = contentPosition;
                Loop(Direction.TowardsStart);
                Loop(Direction.TowardsEnd);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove a calculable child from the Scroll Snap. The distances between items will remain the same and the child will be deleted.
        /// </summary>
        /// <param name="child">The child to be removed.</param>
        public void RemoveChild(RectTransform child)
        {
            if (child == null || !m_ChildrenForSizeFromStartToEnd.Contains(child))
            {
                return;
            }

            RemoveViaCombine(child);

            Destroy(child.gameObject);
        }

        /// <summary>
        /// Remove a calculable child from the Scroll Snap. The child will be deleted.
        /// </summary>
        /// <param name="child">The child to be removed.</param>
        /// <param name="calculateSpacing">The spacing between the end (bottom/right) edge of the previous calculable child, and the start (top/left) edge of the next calculable child.</param>
        /// <param name="lockMode">Determines which calculable items will be "shifted" and which will be "locked" in the same position, relative to the center of the Scroll Snap.</param>
        public void RemoveChild(RectTransform child, float calculateSpacing, LockMode lockMode)
        {
            if (child == null || !m_ChildrenForSizeFromStartToEnd.Contains(child))
            {
                return;
            }

            RemoveViaShift(child, calculateSpacing, lockMode);

            Destroy(child.gameObject);

            ResetContentPos();
        }

        /// <summary>
        /// Remove a calculable child from the Scroll Snap. The distances between items will remain the same and the child will be reparented to the newParent.
        /// </summary>
        /// <param name="child">The child to be removed.</param>
        /// <param name="newParent">The RectTransform the child will be parented to, if the newParent is null the new child will be reparented to the parent Canvas.</param>
        public void RemoveChild(RectTransform child, RectTransform newParent)
        {
            if (child == null || !m_ChildrenForSizeFromStartToEnd.Contains(child))
            {
                return;
            }

            RemoveViaCombine(child);

            SetParentToNewParent(child, newParent);
        }

        /// <summary>
        /// Remove a calculable child from the Scroll Snap. The child will be reparented to the newParent.
        /// </summary>
        /// <param name="child">The child to be removed.</param>
        /// <param name="newParent">The RectTransform the child will be parented to, if the newParent is null the new child will be reparented to the parent Canvas.</param>
        /// <param name="calculateSpacing">The spacing between the end (bottom/right) edge of the previous calculable child, and the start (top/left) edge of the next calculable child.</param>
        /// <param name="lockMode">Determines which calculable items will be "shifted" and which will be "locked" in the same position, relative to the center of the Scroll Snap.</param>
        public void RemoveChild(RectTransform child, RectTransform newParent, float calculateSpacing, LockMode lockMode)
        {
            if (child == null || !m_ChildrenForSizeFromStartToEnd.Contains(child))
            {
                return;
            }

            RemoveViaShift(child, calculateSpacing, lockMode);

            SetParentToNewParent(child, newParent);

            ResetContentPos();
        }
        
        /// <summary>
        /// Scrolls to the position of the supplied snappable child.
        /// </summary>
        /// <param name="child">The child you want the Scroll Snap to scroll to.</param>
        /// <param name="duration">The duration of the scroll in seconds.</param>
        /// <param name="interpolator">Modifies the animation.</param>
        /// <returns></returns>
        public bool ScrollToSnappableChild(RectTransform child, float duration, Interpolator interpolator)
        {
            if (child == null)
            {
                return false;
            }
            
            Vector2 contentPosition;
            GetPositionOfChild(child, out contentPosition);
            if (m_AvailableForSnappingTo.Contains(child))
            {
                contentPosition[inverseAxis] = m_Content.anchoredPosition[inverseAxis];
                m_Scroller.StartScroll(m_Content.anchoredPosition, contentPosition, duration, interpolator);

                m_StartMovementEvent.Invoke(StartMovementEventType.Programmatic);
                int index = 0;
                GetSnapIndexOfChild(child, out index);
                m_TargetItemSelected.Invoke(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Scrolls to the supplied index in the supplied duration of time.
        /// </summary>
        /// <param name="index">The index of the snap position you want to scroll to.</param>
        /// <param name="durationMillis">The duration of the scroll in seconds.</param>
        public void ScrollToSnapPosition(int index, float duration, Interpolator interpolator)
        {
            m_StartMovementEvent.Invoke(StartMovementEventType.Programmatic);
            m_Scroller.ForceFinish();
            Vector2 targetPosition = m_Content.anchoredPosition;
            targetPosition[axis] = m_SnapPositions[index][axis];
            m_Scroller.StartScroll(m_Content.anchoredPosition, targetPosition, duration, interpolator);
            m_TargetItemSelected.Invoke(index);
        }

        /// <summary>
        /// Scrolls to the nearest snap position to the end position in the supplied duration of time.
        /// </summary>
        /// <param name="endPos">The reference end position of the content, in the content's local coordinates.</param>
        /// <param name="durationMillis">The duration of the scroll in seconds.</param>
        public void ScrollToNearestSnapPosition(Vector2 endPos, float duration, Interpolator interpolator)
        {
            m_StartMovementEvent.Invoke(StartMovementEventType.Programmatic);
            m_Scroller.ForceFinish();
            Vector2 targetPosition = m_Content.anchoredPosition;
            targetPosition[axis] = FindClosestSnapPositionToPosition(endPos)[axis];
            m_Scroller.StartScroll(m_Content.anchoredPosition, targetPosition, duration, interpolator);
            m_TargetItemSelected.Invoke(m_SnapPositions.IndexOf(targetPosition));
        }

        /// <summary>
        /// Scrolls to the nearest snap position to the normalized position in the supplied duration of time.
        /// </summary>
        /// <param name="normalizedPos">The reference end position of the content, normalized.</param>
        /// <param name="durationMillis">The duration of the scroll in seconds.</param>
        public void ScrollToNearestSnapPosition(float normalizedPos, float duration, Interpolator interpolator)
        {
            m_StartMovementEvent.Invoke(StartMovementEventType.Programmatic);
            m_Scroller.ForceFinish();
            Vector2 anchoredPos = m_Content.anchoredPosition;
            SetNormalizedPosition(normalizedPos, axis);
            Vector2 targetPosition = m_Content.anchoredPosition;
            targetPosition[axis] = FindClosestSnapPositionToPosition(m_Content.anchoredPosition)[axis];
            m_Scroller.StartScroll(anchoredPos, targetPosition, duration, interpolator);
            m_TargetItemSelected.Invoke(m_SnapPositions.IndexOf(targetPosition));
        }

        /// <summary>
        /// Tells the Scroll Snap whether it should snap to this child. If the child is not calculable it cannot be snapped to.
        /// Used for "Locking" or "Unlocking" Items that are already included in the Scroll Snap. (e.g. in a level/map menu)
        /// If you would like to add/remove a child use the Insert/Remove child functions.
        /// </summary>
        /// <param name="child">The child you would like to change the snappability of.</param>
        /// <param name="snappable">Whether the child should be snapped to or not.</param>
        public void SetChildSnappability(RectTransform child, bool snappable)
        {
            if (m_ChildrenForSnappingFromStartToEnd.Contains(child) == snappable || !m_ChildrenForSizeFromStartToEnd.Contains(child))
            {
                return;
            }

            int calculateIndex = 0;
            GetCalculateIndexOfChild(child, out calculateIndex);

            int snapIndex = 0;
            GetSnapIndexOfChild(child, out snapIndex);

            if (snappable)
            {
                m_ChildrenForSnappingFromStartToEnd.Insert(snapIndex, child);
                m_SnapDistances[LoopIndex(snapIndex - 1, m_SnapDistances.Count)] = GetSnapDistance(snapIndex - 1, snapIndex);
                m_SnapDistances.Insert(snapIndex, GetSnapDistance(snapIndex, snapIndex + 1));
                if (m_FilterModeForSnapPositions == FilterMode.WhiteList)
                {
                    m_SnapPositionsFilter.Add(child);
                }
                else
                {
                    m_SnapPositionsFilter.Remove(child);
                }
            }
            else
            {
                CombineDistance(snapIndex, m_SnapDistances);
                if (m_FilterModeForSnapPositions == FilterMode.WhiteList)
                {
                    m_SnapPositionsFilter.Remove(child);
                }
                else
                {
                    m_SnapPositionsFilter.Add(child);
                }
            }

            m_UpdateSnapPositions = true;
        }

        /// <summary>
        /// Used only in the case of adding a decorative object at runtime (an item that never has been and never will be calculable).
        /// If you would like to remove a calculable item use the RemoveChild functions.
        /// </summary>
        /// <param name="transform">The RectTransform you would like to set uncalculable.</param>
        public void SetRectTransformUncalculable(RectTransform transform)
        {
            if (m_FilterModeForCalculatingSize == FilterMode.BlackList)
            {
                m_CalculatingFilter.Add(transform);
            }
        }

        /// <summary>
        /// Sets the spacing between the child and the next calculable item.
        /// </summary>
        /// <param name="child">The child you would like to change the spacing of.</param>
        /// <param name="spacing">The spacing between the end (bottom/right) edge of the child, and the start (top/left) edge of the next calculable child.</param>
        /// <param name="lockMode">Determines which calculable items will be "shifted" and which will be "locked" in the same position, relative to the center of the Scroll Snap.</param>
        public void SetItemEndSpacing(RectTransform child, float spacing, LockMode lockMode)
        {
            if (child == null || !m_ChildrenForSizeFromStartToEnd.Contains(child))
            {
                return;
            }

            spacing = Mathf.Max(spacing, 0);
            int calculateIndex = 0;
            GetCalculateIndexOfChild(child, out calculateIndex);
            SetReferencePos(GetTrackingChild(calculateIndex, lockMode));

            ShiftChildrenForEndSpacing(calculateIndex, spacing);
            m_CalculateDistances[calculateIndex] = GetCalculateDistance(calculateIndex, spacing, calculateIndex + 1);

            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
            {
                int snapIndex = 0;
                GetSnapIndexOfChild(child, out snapIndex);
                m_SnapDistances[snapIndex] = GetSnapDistance(snapIndex, snapIndex + 1);
            }

            ResetContentPos();
            m_UpdateContentSize = true;
            m_UpdateSnapPositions = true;
        }

        /// <summary>
        /// Sets the spacing between the child at the calculateIndex and the next calculable item.
        /// </summary>
        /// <param name="calculateIndex">The index of the child you would like to modify, based on the calculable items.</param>
        /// <param name="spacing">The spacing between the end (bottom/right) edge of the child, and the start (top/left) edge of the next calculable child.</param>
        /// <param name="lockMode">Determines which calculable items will be "shifted" and which will be "locked" in the same position, relative to the center of the Scroll Snap.</param>
        public void SetItemEndSpacing(int calculateIndex, float spacing, LockMode lockMode)
        {
            spacing = Mathf.Max(spacing, 0);
            calculateIndex = Mathf.Clamp(calculateIndex, 0, m_ChildrenForSizeFromStartToEnd.Count - 1);
            RectTransform child = m_ChildrenForSizeFromStartToEnd[calculateIndex];
            SetReferencePos(GetTrackingChild(calculateIndex, lockMode));

            ShiftChildrenForEndSpacing(calculateIndex, spacing);
            m_CalculateDistances[calculateIndex] = GetCalculateDistance(calculateIndex, spacing, calculateIndex + 1);

            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
            {
                int snapIndex = 0;
                GetSnapIndexOfChild(child, out snapIndex);
                m_SnapDistances[snapIndex] = GetSnapDistance(snapIndex, snapIndex + 1);
            }

            ResetContentPos();
            m_UpdateContentSize = true;
            m_UpdateSnapPositions = true;
        }

        /// <summary>
        /// Sets the spacing between the child and the previous calculable item.
        /// </summary>
        /// <param name="child">The child you would like to change the spacing of.</param>
        /// <param name="spacing">The spacing between the start (top/left) edge of the child, and the end (bottom/right) edge of the previous calculable child.</param>
        /// <param name="lockMode">Determines which calculable items will be "shifted" and which will be "locked" in the same position, relative to the center of the Scroll Snap.</param>
        public void SetItemStartSpacing(RectTransform child, float spacing, LockMode lockMode)
        {
            if (child == null || !m_ChildrenForSizeFromStartToEnd.Contains(child))
            {
                return;
            }

            spacing = Mathf.Max(spacing, 0);
            int calculateIndex = 0;
            GetCalculateIndexOfChild(child, out calculateIndex);
            SetReferencePos(GetTrackingChild(calculateIndex, lockMode));

            ShiftChildrenForEndSpacing(calculateIndex - 1, spacing);
            m_CalculateDistances[LoopIndex(calculateIndex - 1, m_CalculateDistances.Count)] = GetCalculateDistance(calculateIndex - 1, spacing, calculateIndex);

            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
            {
                int snapIndex = 0;
                GetSnapIndexOfChild(child, out snapIndex);
                m_SnapDistances[LoopIndex(snapIndex - 1, m_SnapDistances.Count)] = GetSnapDistance(snapIndex - 1, snapIndex);
            }

            ResetContentPos();
            m_UpdateContentSize = true;
            m_UpdateSnapPositions = true;
        }

        /// <summary>
        /// Sets the spacing between the child and the previous calculable item.
        /// </summary>
        /// <param name="calculateIndex">The index of the child you would like to modify, based on the calculable items.</param>
        /// <param name="spacing">The spacing between the start (top/left) edge of the child, and the end (bottom/right) edge of the previous calculable child.</param>
        /// <param name="lockMode">Determines which calculable items will be "shifted" and which will be "locked" in the same position, relative to the center of the Scroll Snap.</param>
        public void SetItemStartSpacing(int calculateIndex, float spacing, LockMode lockMode)
        {
            spacing = Mathf.Max(spacing, 0);
            calculateIndex = Mathf.Clamp(calculateIndex, 0, m_ChildrenForSizeFromStartToEnd.Count - 1);
            RectTransform child = m_ChildrenForSizeFromStartToEnd[calculateIndex];
            SetReferencePos(GetTrackingChild(calculateIndex, lockMode));

            ShiftChildrenForEndSpacing(calculateIndex - 1, spacing);
            m_CalculateDistances[LoopIndex(calculateIndex - 1, m_CalculateDistances.Count)] = GetCalculateDistance(calculateIndex - 1, spacing, calculateIndex);

            if (m_ChildrenForSnappingFromStartToEnd.Contains(child))
            {
                int snapIndex = 0;
                GetSnapIndexOfChild(child, out snapIndex);
                m_SnapDistances[LoopIndex(snapIndex - 1, m_SnapDistances.Count)] = GetSnapDistance(snapIndex - 1, snapIndex);
            }

            ResetContentPos();
            m_UpdateContentSize = true;
            m_UpdateSnapPositions = true;
        }
        #endregion

        #region Calculations
        private Vector2 FindClosestSnapPositionToPosition(Vector2 contentEndPositon, Direction direction, bool loop)
        {
            EnsureLayoutHasRebuilt();

            Vector2 snapPos = (direction == Direction.TowardsEnd) ? m_SnapPositions[0] : m_SnapPositions[m_SnapPositions.Count - 1];
            int distanceIndex = (direction == Direction.TowardsEnd) ? 0 : m_SnapDistances.Count - 2;
            
            float distance = DistanceOnAxis(contentEndPositon, snapPos, axis);

            while (loop || (distanceIndex > -1 && distanceIndex < m_ChildrenForSnappingFromStartToEnd.Count - 1))
            {
                if (direction == Direction.TowardsEnd)
                {
                    snapPos[axis] -= movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)];
                    if (DistanceOnAxis(contentEndPositon, snapPos, axis) > distance)
                    {
                        snapPos[axis] += movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)]; //revert to previous
                        break;
                    }
                    else
                    {
                        distance = DistanceOnAxis(contentEndPositon, snapPos, axis);
                        distanceIndex++;
                    }
                }
                else
                {
                    snapPos[axis] += movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)];
                    if (DistanceOnAxis(contentEndPositon, snapPos, axis) > distance)
                    {
                        snapPos[axis] -= movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)]; //revert to previous
                        break;
                    }
                    else
                    {
                        distance = DistanceOnAxis(contentEndPositon, snapPos, axis);
                        distanceIndex--;
                    }
                }
            }
            return snapPos;
        }

        private Vector2 FindClosestSnapPositionToPosition(Vector2 contentEndPosition)
        {
            Vector3 closest = Vector3.zero;
            float distance = Mathf.Infinity;

            foreach (Vector2 snapPosition in m_SnapPositions)
            {
                if (DistanceOnAxis(contentEndPosition, snapPosition, axis) < distance)
                {
                    distance = DistanceOnAxis(contentEndPosition, snapPosition, axis);
                    closest = snapPosition;
                }
            }
            return closest;
        }

        private Vector2 FindLastSnapPositionBeforePosition(Vector2 contentEndPosition, Direction direction, bool loop)
        {
            EnsureLayoutHasRebuilt();

            Vector2 snapPos = (direction == Direction.TowardsEnd) ? m_SnapPositions[0] : m_SnapPositions[m_SnapPositions.Count - 1];
            int distanceIndex = (direction == Direction.TowardsEnd) ? 0 : m_SnapDistances.Count - 2;

            while (loop || (distanceIndex > -1 && distanceIndex < m_ChildrenForSnappingFromStartToEnd.Count - 1))
            {
                if (direction == Direction.TowardsEnd)
                {
                    if (ContentPosAIsNearerEnd(snapPos, contentEndPosition))
                    {
                        break;
                    }
                    else
                    {
                        snapPos[axis] -= movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)];
                        distanceIndex++;
                    }
                }
                else
                {
                    if (ContentPosAIsNearerStart(snapPos, contentEndPosition))
                    {
                        break;
                    }
                    else
                    {
                        snapPos[axis] += movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)];
                        distanceIndex--;
                    }
                }
            }

            if (direction == Direction.TowardsEnd)
            {
                snapPos[axis] += movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex - 1, m_SnapDistances.Count)]; //revert to previous
            }
            else
            {
                snapPos[axis] -= movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex + 1, m_SnapDistances.Count)]; //revert to previous
            }

            return snapPos;
        }

        private Vector2 FindNextSnapAfterPosition(Vector2 contentEndPosition, Direction direction, bool loop)
        {
            EnsureLayoutHasRebuilt();
            
            Vector2 snapPos = (direction == Direction.TowardsEnd) ? m_SnapPositions[0] : m_SnapPositions[m_SnapPositions.Count - 1];
            int distanceIndex = (direction == Direction.TowardsEnd) ? 0 : m_SnapDistances.Count - 2;
            
            while (loop || (distanceIndex > -1 && distanceIndex < m_ChildrenForSnappingFromStartToEnd.Count - 1))
            {
                if (direction == Direction.TowardsEnd)
                {
                    if(ContentPosAIsNearerEnd(snapPos, contentEndPosition))
                    {
                        break;
                    }
                    else
                    {
                        snapPos[axis] -= movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)];
                        distanceIndex++;
                    }
                }
                else
                {
                    if (ContentPosAIsNearerStart(snapPos, contentEndPosition))
                    {
                        break;
                    }
                    else
                    {
                        snapPos[axis] += movementDirectionMult * m_SnapDistances[LoopIndex(distanceIndex, m_SnapDistances.Count)];
                        distanceIndex--;
                    }
                }
            }
            return snapPos;
        }

        private int GetSnapIndexOfSnapPosition(Vector2 snapPosition, Direction direction)
        {
            Vector2 pos = (direction == Direction.TowardsEnd) ? m_SnapPositions[0] : m_SnapPositions[m_SnapPositions.Count - 1];
            int index = (direction == Direction.TowardsEnd) ? 0 : m_SnapDistances.Count - 2;
            while (true)
            {
                if (direction == Direction.TowardsEnd)
                {
                    if (ContentPosAIsNearerStart(pos, snapPosition))
                    {
                        pos[axis] -= movementDirectionMult * m_SnapDistances[LoopIndex(index, m_SnapDistances.Count)];
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (ContentPosAIsNearerEnd(pos, snapPosition))
                    {
                        pos[axis] += movementDirectionMult * m_SnapDistances[LoopIndex(index, m_SnapDistances.Count)];
                        index--;
                    }
                    else
                    {
                        index++;
                        break;
                    }
                }
            }

            return LoopIndex(index, m_SnapDistances.Count);
        }

        private Direction GetDirectionFromVelocity(Vector2 velocity, int axis)
        {
            if (axis == 0)
            {
                if (Mathf.Sign(velocity[axis]) > 0)
                {
                    return Direction.TowardsStart;
                }
                else
                {
                    return Direction.TowardsEnd;
                }
            }
            else
            {
                if (Mathf.Sign(velocity[axis]) > 0)
                {
                    return Direction.TowardsEnd;
                }
                else
                {
                    return Direction.TowardsStart;
                }
            }
        }

        private float DistanceOnAxis(Vector2 posOne, Vector2 posTwo, int axis)
        {
            return Mathf.Abs(posOne[axis] - posTwo[axis]);
        }

        private bool TransformAIsNearerStart(RectTransform transformA, RectTransform transformB)
        {
            if (transformA == null)
            {
                return false;
            }

            if (transformB == null)
            {
                return true;
            }

            if (movementDirection == MovementDirection.Horizontal)
            {
                return transformA.anchoredPosition[axis] < transformB.anchoredPosition[axis];
            }
            else
            {
                return transformA.anchoredPosition[axis] > transformB.anchoredPosition[axis];
            }
        }

        private bool ContentPosAIsNearerStart(Vector2 posA, Vector2 posB)
        {
            if (movementDirection == MovementDirection.Horizontal)
            {
                return posA[axis] > posB[axis];
            }
            else
            {
                return posA[axis] < posB[axis];
            }
        }

        private bool ContentPosAIsNearerEnd(Vector2 posA, Vector2 posB)
        {
            if (movementDirection == MovementDirection.Horizontal)
            {
                return posA[axis] < posB[axis];
            }
            else
            {
                return posA[axis] > posB[axis];
            }
        }

        private float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        private Interpolator GetInterpolator(InterpolatorType interpolatorType, float tension)
        {
            Interpolator interpolator = new Scroller.ViscousFluidInterpolator();
            switch (interpolatorType)
            {
                case InterpolatorType.Accelerate:
                    interpolator = new Scroller.AccelerateInterpolator();
                    break;
                case InterpolatorType.AccelerateDecelerate:
                    interpolator = new Scroller.AccelerateDecelerateInterpolator();
                    break;
                case InterpolatorType.Anticipate:
                    interpolator = new Scroller.AnticipateInterpolator(tension);
                    break;
                case InterpolatorType.AnticipateOvershoot:
                    interpolator = new Scroller.AnticipateOvershootInterpolator(tension);
                    break;
                case InterpolatorType.Decelerate:
                    interpolator = new Scroller.DecelerateInterpolator();
                    break;
                case InterpolatorType.DecelerateAccelerate:
                    interpolator = new Scroller.DecelerateAccelerateInterpolator();
                    break;
                case InterpolatorType.Linear:
                    interpolator = new Scroller.LinearInterpolator();
                    break;
                case InterpolatorType.Overshoot:
                    interpolator = new Scroller.OvershootInterpolator(tension);
                    break;
            }

            return interpolator;
        }
        
        /// <summary>
        /// The calculate distances must be calculated and both children must be added before calling this.
        /// </summary>
        private float GetSnapDistance(int startSnapIndex, int endSnapIndex)
        {
            int startCalculateIndex = 0;
            GetCalculateIndexOfChild(m_ChildrenForSnappingFromStartToEnd[LoopIndex(startSnapIndex, m_ChildrenForSnappingFromStartToEnd.Count)], out startCalculateIndex);

            int endCalculateIndex = 0;
            GetCalculateIndexOfChild(m_ChildrenForSnappingFromStartToEnd[LoopIndex(endSnapIndex, m_ChildrenForSnappingFromStartToEnd.Count)], out endCalculateIndex);

            float distanceToSnap = 0;
            while (true)
            {
                distanceToSnap += m_CalculateDistances[LoopIndex(startCalculateIndex, m_CalculateDistances.Count)];

                if (LoopIndex(startCalculateIndex + 1, m_CalculateDistances.Count) == endCalculateIndex)
                {
                    break;
                }

                startCalculateIndex++;
            }
            return distanceToSnap;
        }

        /// <summary>
        /// Make sure that both of the children have been added to m_ChildrenForSizeFromStartToEnd before calling.
        /// </summary>
        private float GetCalculateDistance(int startCalculateIndex, float spacing, int endCalculateIndex)
        {
            Vector3[] childOneCorners = new Vector3[4];
            m_ChildrenForSizeFromStartToEnd[LoopIndex(startCalculateIndex, m_ChildrenForSizeFromStartToEnd.Count)].GetLocalCorners(childOneCorners);

            Vector3[] childTwoCorners = new Vector3[4];
            m_ChildrenForSizeFromStartToEnd[LoopIndex(endCalculateIndex, m_ChildrenForSizeFromStartToEnd.Count)].GetLocalCorners(childTwoCorners);

            return DistanceOnAxis(childOneCorners[3], Vector2.zero, axis) + spacing + DistanceOnAxis(childTwoCorners[1], Vector2.zero, axis);
        }
        
        private int LoopIndex(int index, int count)
        {
            if (count == 0)
            {
                return 0;
            }

            if (index >= count)
            {
                return index % count;
            }

            if (index < 0)
            {
                int test = (index % count == 0) ? 0 : count + (index % count);
                return test;
            }

            return index;
        }
        #endregion

        #region Control
        public override bool IsActive()
        {
            return base.IsActive() && m_Content != null && m_AvailableForSnappingTo.Count > 0;
        }

        public virtual void SetLayoutHorizontal()
        {
        }

        public virtual void SetLayoutVertical()
        {
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();
        }

        private void RebuildLayoutGroups()
        {
            if (contentIsLayoutGroup && m_LayoutGroup.enabled)
            {
                m_LayoutGroup.CalculateLayoutInputHorizontal();
                m_LayoutGroup.CalculateLayoutInputVertical();
                m_LayoutGroup.SetLayoutHorizontal();
                m_LayoutGroup.SetLayoutVertical();
            }
        }

        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.PostLayout)
            {
                UpdateBounds();
                UpdateScrollbars(Vector2.zero);
                UpdatePrevData();

                m_HasRebuiltLayout = true;
            }
        }

        public virtual void LayoutComplete()
        { }

        public virtual void GraphicUpdateComplete()
        { }

        private void EnsureLayoutHasRebuilt()
        {
            if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
            {
                Canvas.ForceUpdateCanvases();
            }
        }

        private void SetHorizontalNormalizedPosition(float value) { SetNormalizedPosition(value, 0); }
        private void SetVerticalNormalizedPosition(float value) { SetNormalizedPosition(value, 1); }

        private void SetNormalizedPosition(float value, int axis)
        {
            EnsureLayoutHasRebuilt();
            UpdateBounds();
            // How much the content is larger than the view.
            float scrollableLength = m_ContentBounds.size[axis] - m_ViewBounds.size[axis];
            //the amount of content below the left corner of the viewbounds
            float amountBelowLeftCorner = (axis == 0) ? value * scrollableLength : (1 - value) * scrollableLength;
            // Where the position of the lower left corner of the content bounds should be, in the space of the view.
            float contentBoundsMinPosition = m_ViewBounds.min[axis] - amountBelowLeftCorner;
            // The new content localPosition, in the space of the view.
            float newLocalPosition = m_Content.localPosition[axis] + contentBoundsMinPosition - m_ContentBounds.min[axis];

            Vector3 localPosition = m_Content.localPosition;
            if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
            {
                localPosition[axis] = newLocalPosition;
                m_Content.localPosition = localPosition;
                UpdateBounds();
            }
        }

        private void UpdateBounds()
        {
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();

            if (m_Content == null)
            {
                return;
            }

            // Make sure content bounds are at least as large as view by adding padding if not.
            // One might think at first that if the content is smaller than the view, scrolling should be allowed.
            // However, that's not how scroll views normally work.
            // Scrolling is *only* possible when content is *larger* than view.
            // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
            // E.g. if pivot is at top, bounds are expanded downwards.
            // This also works nicely when ContentSizeFitter is used on the content.
            Vector3 contentSize = m_ContentBounds.size;
            Vector3 contentPos = m_ContentBounds.center;
            Vector3 excess = m_ViewBounds.size - contentSize;
            if (excess.x > 0)
            {
                contentPos.x -= excess.x * (m_Content.pivot.x - 0.5f);
                contentSize.x = m_ViewBounds.size.x;
            }
            if (excess.y > 0)
            {
                contentPos.y -= excess.y * (m_Content.pivot.y - 0.5f);
                contentSize.y = m_ViewBounds.size.y;
            }

            m_ContentBounds.size = contentSize;
            m_ContentBounds.center = contentPos;
        }

        private readonly Vector3[] m_ContentWorldCorners = new Vector3[4];
        private Bounds GetBounds()
        {
            if (m_Content == null)
            {
                return new Bounds();
            }

            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var toLocal = viewRect.worldToLocalMatrix;
            m_Content.GetWorldCorners(m_ContentWorldCorners);
            for (int j = 0; j < 4; j++)
            {
                Vector3 v = toLocal.MultiplyPoint3x4(m_ContentWorldCorners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        private Vector2 CalculateOffset(Vector2 delta)
        {
            Vector2 offset = Vector2.zero;

            Vector2 min = m_ContentBounds.min;
            Vector2 max = m_ContentBounds.max;

            if (movementDirection == MovementDirection.Horizontal || (movementDirection == MovementDirection.Vertical && !m_LockOtherDirection))
            {
                min.x += delta.x;
                max.x += delta.x;
                if (min.x > m_ViewBounds.min.x)
                {
                    offset.x = m_ViewBounds.min.x - min.x;
                }
                else if (max.x < m_ViewBounds.max.x)
                {
                    offset.x = m_ViewBounds.max.x - max.x;
                }
            }

            if (movementDirection == MovementDirection.Vertical || (movementDirection == MovementDirection.Horizontal && !m_LockOtherDirection))
            {
                min.y += delta.y;
                max.y += delta.y;
                if (max.y < m_ViewBounds.max.y)
                {
                    offset.y = m_ViewBounds.max.y - max.y;
                }
                else if (min.y > m_ViewBounds.min.y)
                {
                    offset.y = m_ViewBounds.min.y - min.y;
                }
            }

            return offset;
        }

        private void UpdateScrollbars(Vector2 offset)
        {
            if (m_HorizontalScrollbar)
            {
                if (m_ContentBounds.size.x > 0)
                {
                    m_HorizontalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x)) / m_ContentBounds.size.x);
                }
                else
                {
                    m_HorizontalScrollbar.size = 1;
                }

                m_HorizontalScrollbar.value = horizontalNormalizedPosition;
            }

            if (m_VerticalScrollbar)
            {
                if (m_ContentBounds.size.y > 0)
                {
                    m_VerticalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y)) / m_ContentBounds.size.y);
                }
                else
                {
                    m_VerticalScrollbar.size = 1;
                }

                m_VerticalScrollbar.value = verticalNormalizedPosition;
            }
        }

        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            if (m_LockOtherDirection)
            {
                if (movementDirection == MovementDirection.Vertical)
                {
                    position.x = m_Content.anchoredPosition.x;
                }
                if (movementDirection == MovementDirection.Horizontal)
                {
                    position.y = m_Content.anchoredPosition.y;
                }
            }

            if (position != m_Content.anchoredPosition)
            {
                m_Content.anchoredPosition = position;
                UpdateBounds();
            }
        }

        private Vector2 m_ReferencePos;
        private RectTransform m_TrackingChild;
        private void SetReferencePos(RectTransform trackingChild)
        {
            m_TrackingChild = trackingChild;
            m_ReferencePos = viewRect.InverseTransformPoint(trackingChild.position);
        }

        private Vector2 ResetContentPos()
        {
            Vector2 newPos = viewRect.InverseTransformPoint(m_TrackingChild.position);
            Vector2 offset = m_ReferencePos - newPos;
            m_Content.anchoredPosition = m_Content.anchoredPosition + offset;
            return offset;
        }

        private void CheckLayoutUpdates()
        {
            if (m_UpdateContentSize)
            {
                m_UpdateContentSize = false;
                ResizeContent();
            }

            if (m_UpdateSnapPositions)
            {
                m_UpdateSnapPositions = false;
                GetSnapPositions();
            }
        }

        private void UpdatePrevData()
        {
            if (m_Content == null)
            {
                m_PrevPosition = Vector2.zero;
            }
            else
            {
                m_PrevPosition = m_Content.anchoredPosition;
            }

            m_PrevViewBounds = m_ViewBounds;
            m_PrevContentBounds = m_ContentBounds;
            m_PrevClosestItem = closestItem;
            m_PrevScrolling = !m_Scroller.isFinished;
        }

        protected void SetDirtyCaching()
        {
            if (!IsActive())
            {
                return;
            }

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        private Transform GetCanvasTransform(Transform transform)
        {
            if (transform.GetComponent<Canvas>() != null)
            {
                return transform;
            }
            else
            {
                if (transform.parent != null)
                {
                    return GetCanvasTransform(transform.parent);
                }
                else
                {
                    return transform;
                }
            }
        }
        #endregion

        #region Virtual Functions
        /// <summary>
        /// Override this method to impliment a custom interpolator for non-programatic StartMovementEventTypes.
        /// Custom interpolators can be directly passed to programatic events.
        /// This method is called before the Directional Scroll Snap starts any animations.
        /// </summary>
        /// <param name="eventType">The input event for this particular animation.</param>
        /// <param name="interpolator">The interpolator that you need to set to your custom interpolator</param>
        public virtual void ImplimentCustomInterpolator(StartMovementEventType eventType, ref Interpolator interpolator) { }
        #endregion

        private void OnDrawGizmos()
        {

            if (m_DrawGizmos)
            {
                Vector3[] corners = new Vector3[4];
                m_Content.GetWorldCorners(corners);

                Vector3 topLeftWorld = corners[1];
                Vector3 topRightWorld = corners[2];
                Vector3 bottomRightWorld = corners[3];
                Vector3 bottomLeftWorld = corners[0];

                Gizmos.color = Color.white;
                Gizmos.DrawLine(topLeftWorld, topRightWorld);
                Gizmos.DrawLine(topRightWorld, bottomRightWorld);
                Gizmos.DrawLine(bottomRightWorld, bottomLeftWorld);
                Gizmos.DrawLine(bottomLeftWorld, topLeftWorld);

                Vector3 topDirection = topRightWorld - topLeftWorld;
                Vector3 leftDirection = bottomLeftWorld - topLeftWorld;
                
                //Draw Snap Positions
                foreach (RectTransform child in m_Content)
                {
                    if (m_AvailableForSnappingTo.Contains(child))
                    {
                        Gizmos.color = Color.cyan;
                        if (m_MovementDirection == MovementDirection.Horizontal)
                        {
                            Gizmos.DrawRay(child.position, leftDirection.normalized * GetGizmoSize(child.position) * .5f);
                            Gizmos.DrawRay(child.position, -(leftDirection.normalized * GetGizmoSize(child.position) * .5f));
                        }
                        else
                        {
                            Gizmos.DrawRay(child.position, topDirection.normalized * GetGizmoSize(child.position) * .5f);
                            Gizmos.DrawRay(child.position, -(topDirection.normalized * GetGizmoSize(child.position) * .5f));
                        }
                    }
                }

                ///Draw Looped Item
                if (m_Loop && m_ChildrenForSizeFromStartToEnd.Count > 0 && !Application.isPlaying)
                {
                    Vector3[] firstChildCorners = new Vector3[4];
                    float distance = GetCalculateDistance(m_ChildrenForSizeFromStartToEnd.Count - 1, m_EndSpacing, 0);
                    firstCalculateChild.GetLocalCorners(firstChildCorners);

                    Vector2 matrixPos = Vector2.zero;
                    matrixPos[axis] = lastCalculateChild.position[axis] + (movementDirectionMult * distance);
                    matrixPos[inverseAxis] = firstCalculateChild.position[inverseAxis];

                    Matrix4x4 matrix = Matrix4x4.TRS(matrixPos, firstCalculateChild.rotation, firstCalculateChild.localScale);

                    Vector3[] gizmoCorners = new Vector3[4];
                    gizmoCorners[0] = matrix.MultiplyPoint3x4(firstChildCorners[0]);
                    gizmoCorners[1] = matrix.MultiplyPoint3x4(firstChildCorners[1]);
                    gizmoCorners[2] = matrix.MultiplyPoint3x4(firstChildCorners[2]);
                    gizmoCorners[3] = matrix.MultiplyPoint3x4(firstChildCorners[3]);

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(gizmoCorners[1], gizmoCorners[2]);
                    Gizmos.DrawLine(gizmoCorners[2], gizmoCorners[3]);
                    Gizmos.DrawLine(gizmoCorners[3], gizmoCorners[0]);
                    Gizmos.DrawLine(gizmoCorners[0], gizmoCorners[1]);
                }
            }
        }

        private float GetGizmoSize(Vector3 position)
        {
            Camera current = Camera.current;
            position = Gizmos.matrix.MultiplyPoint(position);

            if (current)
            {
                Transform transform = current.transform;
                Vector3 position2 = transform.position;
                float z = Vector3.Dot(position - position2, transform.TransformDirection(new Vector3(0f, 0f, 1f)));
                Vector3 a = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(0f, 0f, z)));
                Vector3 b = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(1f, 0f, z)));
                float magnitude = (a - b).magnitude;
                return 80f / Mathf.Max(magnitude, 0.0001f);
            }

            return 20f;
        }

        private void Validate()
        {
            m_Friction = Mathf.Clamp(m_Friction, .1f, .999f);
            m_Tension = Mathf.Max(m_Tension, 0);
            m_MinDuration = Mathf.Max(m_MinDuration, .001f);
            m_MaxDuration = Math.Max(m_MaxDuration, Mathf.Max(m_MinDuration, .001f));

            m_ButtonAnimationDuration = Mathf.Max(m_ButtonAnimationDuration, .001f);
            m_ButtonItemsToMoveBy = Mathf.Max(m_ButtonItemsToMoveBy, 1);
            m_ButtonTension = Mathf.Max(m_ButtonTension, 0);

            m_ScrollBarFriction = Mathf.Clamp(m_ScrollBarFriction, .1f, .999f);
            m_ScrollBarTension = Mathf.Max(m_ScrollBarTension, 0);

            m_ScrollSensitivity = Mathf.Max(m_ScrollSensitivity, 0);
            m_ScrollDelay = Mathf.Max(m_ScrollDelay, 0);
            m_ScrollFriction = Mathf.Clamp(m_ScrollFriction, .1f, .999f);
            m_ScrollTension = Mathf.Max(m_ScrollTension, 0);

            m_EndSpacing = Mathf.Max(m_EndSpacing, 0);
            if (m_ChildrenForSizeFromStartToEnd.Count == 1)
            {
                m_EndSpacing = Mathf.Max(m_EndSpacing, (int)m_ViewBounds.size[axis]);
            }

            if (contentIsHorizonalLayoutGroup)
            {
                m_MovementDirection = MovementDirection.Horizontal;
            }

            if (contentIsVerticalLayoutGroup)
            {
                m_MovementDirection = MovementDirection.Vertical;
            }

            SetDirtyCaching();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Validate();
        }

        [MenuItem("GameObject/UI/ScrollSnaps/DirectionalScrollSnap", false, 10)]
        private static void CreateDirectionalScrollSnap(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;

            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas == null || !canvas.gameObject.activeInHierarchy)
                {
                    parent = new GameObject("Canvas");
                    parent.layer = LayerMask.NameToLayer("UI");
                    canvas = parent.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    parent.AddComponent<CanvasScaler>();
                    parent.AddComponent<GraphicRaycaster>();
                    Undo.RegisterCreatedObjectUndo(parent, "Create " + parent.name);

                    EventSystem evsy = FindObjectOfType<EventSystem>();
                    if (evsy == null || !evsy.gameObject.activeInHierarchy)
                    {
                        GameObject eventSystem = new GameObject("EventSystem");
                        eventSystem.AddComponent<EventSystem>();
                        eventSystem.AddComponent<StandaloneInputModule>();

                        Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
                    }
                }
                else
                {
                    parent = canvas.gameObject;
                }
            }

            int numChildren = 2;

            GameObject GO = new GameObject("Directional Scroll Snap");
            RectTransform rectTransform = GO.AddComponent<RectTransform>();
            DirectionalScrollSnap scrollSnap = GO.AddComponent<DirectionalScrollSnap>();
            Image image = GO.AddComponent<Image>();

            GameObject content = new GameObject("Content");
            RectTransform contentRectTransform = content.AddComponent<RectTransform>();
            Image contentImage = content.AddComponent<Image>();


            GO.transform.SetParent(parent.transform, false);
            rectTransform.sizeDelta = new Vector2(200, 200);
            scrollSnap.content = contentRectTransform;
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            image.type = Image.Type.Sliced;
            image.color = Color.red;

            content.transform.SetParent(GO.transform, false);
            contentRectTransform.anchorMin = new Vector2(0, 1);
            contentRectTransform.anchorMax = new Vector2(0, 1);
            contentRectTransform.sizeDelta = new Vector2(200 + (150 * (numChildren - 1)), 200);
            contentRectTransform.anchoredPosition = new Vector2(100, -100);
            contentImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            contentImage.type = Image.Type.Sliced;
            contentImage.color = new Color(0, 0, 1, .5f);

            for (int i = 0; i < numChildren; i++)
            {
                GameObject child = new GameObject("Child Item");
                RectTransform childRectTransform = child.AddComponent<RectTransform>();
                child.AddComponent<Image>();

                child.transform.SetParent(content.transform, false);
                childRectTransform.anchorMin = new Vector2(0, 1);
                childRectTransform.anchorMax = new Vector2(0, 1);
                childRectTransform.sizeDelta = new Vector2(100, 100);
                childRectTransform.anchoredPosition = new Vector2(100 + (150 * i), -100);
            }

            GameObjectUtility.SetParentAndAlign(GO, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(GO, "Create " + GO.name);
            Selection.activeObject = GO;


            // Find the best scene view
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
            {
                sceneView = SceneView.sceneViews[0] as SceneView;
            }

            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null || sceneView.camera == null)
            {
                return;
            }

            // Create world space Plane from canvas position.
            RectTransform canvasRTransform = parent.GetComponent<RectTransform>();
            Vector2 localPlanePosition;
            Camera camera = sceneView.camera;
            Vector3 position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
            {
                // Adjust for canvas pivot
                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

                // Adjust for anchoring
                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * rectTransform.anchorMin.x;
                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * rectTransform.anchorMin.y;

                Vector3 minLocalPosition;
                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + rectTransform.sizeDelta.x * rectTransform.pivot.x;
                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + rectTransform.sizeDelta.y * rectTransform.pivot.y;

                Vector3 maxLocalPosition;
                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - rectTransform.sizeDelta.x * rectTransform.pivot.x;
                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - rectTransform.sizeDelta.y * rectTransform.pivot.y;

                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }

            rectTransform.anchoredPosition = position;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
        }
#endif

        protected class ScrollBarEventsListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
        {
            public Action<PointerEventData> onPointerDown;
            public Action<PointerEventData> onPointerUp;

            public virtual void OnPointerDown(PointerEventData ped)
            {
                onPointerDown(ped);
            }

            public virtual void OnPointerUp(PointerEventData ped)
            {
                onPointerUp(ped);
            }
        }
    }
}
