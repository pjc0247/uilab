//Dependencies:
// - Scroller: Source > Scripts > HelperClasses
// - OmniDirectionalScrollSnapEditor: Source > Editor (optional)

//Contributors:
//BeksOmega

using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.UI.ScrollSnaps
{
    [HelpURL("https://bitbucket.org/beksomega/unityuiscrollsnaps/wiki/Components/OmniDirectionalScrollSnap")]
    [AddComponentMenu("UI/Scroll Snaps/OmniDirectional Scroll Snap")]
    [ExecuteInEditMode]
    public class OmniDirectionalScrollSnap : UIBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, ICanvasElement, ILayoutGroup, IScrollHandler
    {

        #region Variables
        public enum MovementType
        {
            Clamped,
            Elastic
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

        public enum ScrollWheelDirection
        {
            Horizontal,
            Vertical
        }

        public enum StartMovementEventType
        {
            Touch,
            ScrollBar,
            OnScroll,
            Programmatic
        }

        public enum RelativeDirection
        {
            Above,
            Left,
            Below,
            Right
        }

        [Serializable]
        public class Vector2Event : UnityEvent<Vector2> { }
        [Serializable]
        public class StartMovementEvent : UnityEvent<StartMovementEventType> { }
        [Serializable]
        public class RectTransformEvent : UnityEvent<RectTransform> { }

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
            }
        }

        [SerializeField]
        private MovementType m_MovementType;
        public MovementType movementType { get { return m_MovementType; } set { m_MovementType = value; } }

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
        private InterpolatorType m_InterpolatorType = InterpolatorType.Decelerate;
        public InterpolatorType interpolator
        {
            get
            {
                return m_InterpolatorType;
            }
            set
            {
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
        private float m_MaxDuration = 2;
        public float maxDuration { get { return m_MaxDuration; }  set { m_MaxDuration = value; } }

        [SerializeField]
        private bool m_AllowTouchInput = true;
        public bool allowTouchInput { get { return m_AllowTouchInput; } set { m_AllowTouchInput = value; } }

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
        private ScrollWheelDirection m_ScrollWheelDirection = ScrollWheelDirection.Vertical;
        public ScrollWheelDirection scrollWheelDirection { get { return m_ScrollWheelDirection; } set { m_ScrollWheelDirection = value; } }

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
        public bool addInactiveChildrenToCalculatingFilter { get { return m_AddInactiveChildrenToCalculatingFilter; } }

        [SerializeField]
        private FilterMode m_FilterModeForCalculatingSize;
        public FilterMode calculateSizeFilterMode { get { return m_FilterModeForCalculatingSize; } }

        [SerializeField]
        private List<RectTransform> m_CalculatingFilter = new List<RectTransform>();
        public List<RectTransform> calculatingFilter { get { return m_CalculatingFilter; } }

        [SerializeField]
        private bool m_AddInactiveChildrenToSnapPositionsFilter;
        public bool addInactiveChildrenToSnapPositionsFilter { get { return m_AddInactiveChildrenToSnapPositionsFilter; } }

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

        [SerializeField]
        private Vector2Event m_OnValueChanged = new Vector2Event();
        public Vector2Event onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        [SerializeField]
        private StartMovementEvent m_StartMovementEvent = new StartMovementEvent();
        public StartMovementEvent startMovementEvent { get { return m_StartMovementEvent; } set { m_StartMovementEvent = value; } }

        [SerializeField]
        private RectTransformEvent m_ClosestSnapPositionChanged = new RectTransformEvent();
        public RectTransformEvent closestSnapPositionChanged { get { return m_ClosestSnapPositionChanged; } set { m_ClosestSnapPositionChanged = value; } }

        [SerializeField]
        private RectTransformEvent m_SnappedToItem = new RectTransformEvent();
        public RectTransformEvent snappedToItem { get { return m_SnappedToItem; } set { m_SnappedToItem = value; } }

        [SerializeField]
        private RectTransformEvent m_TargetItemSelected = new RectTransformEvent();
        public RectTransformEvent targetItemSelected { get { return m_TargetItemSelected; } set { m_TargetItemSelected = value; } }

        [SerializeField]
        private bool m_DrawGizmos = false;


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

        private RectTransform m_ClosestItem;
        public RectTransform closestItem { get { return m_ClosestItem; } }

        private List<RectTransform> m_ChildrenForSizeFromTopToBottom = new List<RectTransform>();
        public List<RectTransform> calculateChildrenTopToBottom { get { return m_ChildrenForSizeFromLeftToRight; } }

        private List<RectTransform> m_ChildrenForSizeFromLeftToRight = new List<RectTransform>();
        public List<RectTransform> calculateChildrenLeftToRight { get { return m_ChildrenForSizeFromLeftToRight; } }

        private List<RectTransform> m_ChildrenForSnappingFromTopToBottom = new List<RectTransform>();
        public List<RectTransform> snapChildrenTopToBottom { get { return m_ChildrenForSnappingFromTopToBottom; } }

        private List<RectTransform> m_ChildrenForSnappingFromLeftToRight = new List<RectTransform>();
        public List<RectTransform> snapChildrenLeftToRight { get { return m_ChildrenForSnappingFromLeftToRight; } }


        private string filterWhitelistException = "The {0} is set to whitelist and is either empty or contains an empty object. You probably need to assign a child to the {0} or set the {0} to blacklist.";
        private string availableChildrenListEmptyException = "The Content has no children available for {0}. This is probably because they are all blacklisted. You should check what children you have blacklisted in your item filters and if you have Add Inactive Children checked.";
        private string contentHasNoChildrenException = "The Content has no children so it is unable to snap. You should assign children to the Content or choose a new RectTransform for the Content.";

        private DrivenRectTransformTracker m_Tracker;
        private Scroller m_Scroller = new Scroller();

        private List<RectTransform> m_AvailableForCalculating = new List<RectTransform>();
        private List<RectTransform> m_AvailableForSnappingTo = new List<RectTransform>();

        private List<Vector2> m_SnapPositions = new List<Vector2>();

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
        private bool m_UpdateChildren;
        private bool m_UpdateContentSize;
        private bool m_UpdateSnapPositions;

        private Vector2 m_MinPos;
        private Vector2 m_MaxPos;

        private bool m_WaitingForEndScrolling;
        private float m_TimeOfLastScroll;

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
        
        private RectTransform leftChild
        {
            get
            {
                return m_ChildrenForSizeFromLeftToRight[0];
            }
        }
        
        private RectTransform rightChild
        {
            get
            {
                return m_ChildrenForSizeFromLeftToRight[m_ChildrenForSizeFromLeftToRight.Count - 1];
            }
        }
        
        private RectTransform topChild
        {
            get
            {
                return m_ChildrenForSizeFromTopToBottom[0];
            }
        }
        
        private RectTransform bottomChild
        {
            get
            {
                return m_ChildrenForSizeFromTopToBottom[m_ChildrenForSizeFromTopToBottom.Count - 1];
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
            JumpToSnappableChild(m_StartItem);

            if (m_HorizontalScrollbar)
            {
                m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
                if (Application.isPlaying)
                {
                    m_HorizontalScrollbarEventsListener = m_HorizontalScrollbar.gameObject.AddComponent<ScrollBarEventsListener>();
                    m_HorizontalScrollbarEventsListener.onPointerDown += ScrollBarPointerDown;
                    m_HorizontalScrollbarEventsListener.onPointerUp += ScrollBarPointerUp;
                }
            }
            if (m_VerticalScrollbar)
            {
                m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
                if (Application.isPlaying)
                {
                    m_VerticalScrollBarEventsListener = m_VerticalScrollbar.gameObject.AddComponent<ScrollBarEventsListener>();
                    m_VerticalScrollBarEventsListener.onPointerDown += ScrollBarPointerDown;
                    m_VerticalScrollBarEventsListener.onPointerUp += ScrollBarPointerUp;
                }
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
        /// Updates the size and snap positions of the scroll snap. Call this whenever you change filters, add new children to the content, ect.
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
            
            Vector2 childOneOrigPosTransformLocalSpace = viewRect.InverseTransformPoint(leftChild.position);

            ResizeContent();
            GetSnapPositions();
            
            Vector2 childOneNewPosTransformLocalSpace = viewRect.InverseTransformPoint(leftChild.position);
            Vector2 offset = childOneOrigPosTransformLocalSpace - childOneNewPosTransformLocalSpace;
            m_Content.anchoredPosition = m_Content.anchoredPosition + offset;
        }

        private void GetValidChildren()
        {
            m_AvailableForCalculating.Clear();
            m_AvailableForSnappingTo.Clear();

            if (m_Content.childCount < 1)
            {
                throw (new MissingReferenceException(contentHasNoChildrenException));
            }

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

        private void GetChildrenFromStartToEnd()
        {
            m_ChildrenForSizeFromTopToBottom.Clear();
            m_ChildrenForSizeFromLeftToRight.Clear();
            foreach (RectTransform child in m_Content)
            {
                if (m_AvailableForCalculating.Contains(child))
                {
                    int leftRightInsert = m_ChildrenForSizeFromLeftToRight.Count;
                    int topBottomInsert = m_ChildrenForSizeFromTopToBottom.Count;
                    for (int i = 0; i < m_ChildrenForSizeFromLeftToRight.Count; i++)
                    {
                        if (child.anchoredPosition.x < m_ChildrenForSizeFromLeftToRight[i].anchoredPosition.x)
                        {
                            leftRightInsert = i;
                            break;
                        }
                    }
                    for (int i = 0; i < m_ChildrenForSizeFromTopToBottom.Count; i++)
                    {
                        if (child.anchoredPosition.y > m_ChildrenForSizeFromTopToBottom[i].anchoredPosition.y)
                        {
                            topBottomInsert = i;
                            break;
                        }
                    }
                    m_ChildrenForSizeFromLeftToRight.Insert(leftRightInsert, child);
                    m_ChildrenForSizeFromTopToBottom.Insert(topBottomInsert, child);
                }
            }

            foreach(RectTransform child in m_ChildrenForSizeFromTopToBottom)
            {
                if (m_AvailableForSnappingTo.Contains(child))
                {
                    m_ChildrenForSnappingFromTopToBottom.Add(child);
                }
            }

            foreach(RectTransform child in m_ChildrenForSizeFromLeftToRight)
            {
                if (m_AvailableForSnappingTo.Contains(child))
                {
                    m_ChildrenForSnappingFromLeftToRight.Add(child);
                }
            }
        }

        private void ResizeContent()
        {
            UpdateBounds();
            float halfViewRectX = m_ViewBounds.extents.x;
            float halfViewRectY = -m_ViewBounds.extents.y;
            int leftChildOffset = (int)(halfViewRectX - leftChild.anchoredPosition.x);
            int rightChildOffset = (int)(halfViewRectX - (m_Content.sizeDelta.x - rightChild.anchoredPosition.x));
            int topChildOffset = (int)(halfViewRectY - topChild.anchoredPosition.y);
            int bottomChildOffset = (int)(halfViewRectY - (-m_Content.sizeDelta.y - bottomChild.anchoredPosition.y));

            if (!Application.isPlaying && contentIsLayoutGroup && m_LayoutGroup.enabled)
            {
                m_LayoutGroup.padding.left = m_LayoutGroup.padding.left + leftChildOffset;
                m_LayoutGroup.padding.right = m_LayoutGroup.padding.right + rightChildOffset;
                m_LayoutGroup.padding.top = m_LayoutGroup.padding.top - topChildOffset;
                m_LayoutGroup.padding.bottom = m_LayoutGroup.padding.bottom - bottomChildOffset;
                m_Content.sizeDelta = new Vector2(m_Content.sizeDelta.x + leftChildOffset + rightChildOffset, m_Content.sizeDelta.y - topChildOffset - bottomChildOffset);
                RebuildLayoutGroups();
            }
            else
            {
                foreach (RectTransform child in m_ChildrenForSizeFromLeftToRight)
                {
                    child.anchoredPosition = new Vector2(child.anchoredPosition.x + leftChildOffset, child.anchoredPosition.y + topChildOffset);
                }
                float totalSizeX = Mathf.Abs(rightChild.anchoredPosition.x) + Mathf.Abs(halfViewRectX);
                float totalSizeY = Mathf.Abs(bottomChild.anchoredPosition.y) + Mathf.Abs(halfViewRectY);
                m_Content.sizeDelta = new Vector2(totalSizeX, totalSizeY);
            }
            SetNormalizedPosition(new Vector2(1, 0));
            m_MinPos = m_Content.anchoredPosition;
            SetNormalizedPosition(new Vector2(0, 1));
            m_MaxPos = m_Content.anchoredPosition;
        }

        private void GetSnapPositions()
        {
            m_SnapPositions.Clear();
            foreach (RectTransform child in m_AvailableForSnappingTo)
            {
                Vector2 normalizedPosition;
                GetNormalizedPositionOfChild(child, out normalizedPosition);
                SetNormalizedPosition(normalizedPosition);
                m_SnapPositions.Add(m_Content.anchoredPosition);
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
            Vector2 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
            m_Velocity = Vector2.Lerp(m_Velocity, newVelocity, deltaTime * 10);

            m_ClosestItem = GetClosestSnappableChildToPosition(m_Content.anchoredPosition);

            if (m_Content.anchoredPosition != m_PrevPosition)
            {
                m_OnValueChanged.Invoke(new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition));
            }
            if (m_ClosestItem != m_PrevClosestItem)
            {
                m_ClosestSnapPositionChanged.Invoke(m_ClosestItem);
            }
            if (m_Scroller.isFinished && m_PrevScrolling)
            {
                m_SnappedToItem.Invoke(m_ClosestItem);
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
        }

        private void ScrollBarPointerUp(PointerEventData ped)
        {
            SelectSnapPos(StartMovementEventType.ScrollBar);
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
            Vector2 position = m_ContentStartPosition + pointerDelta;

            // Offset to get content into place in the view.
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

        public virtual void OnBeginDrag(PointerEventData ped)
        {
            if (!IsActive() || !m_AllowTouchInput)
            {
                return;
            }

            m_StartMovementEvent.Invoke(StartMovementEventType.Touch);

            UpdateBounds();

            m_PointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, ped.position, ped.pressEventCamera, out m_PointerStartLocalCursor);
            m_ContentStartPosition = m_Content.anchoredPosition;

            m_Scroller.ForceFinish();
            m_Velocity = Vector2.zero;
        }

        public virtual void OnEndDrag(PointerEventData ped)
        {
            if (!IsActive() || !m_AllowTouchInput)
            {
                return;
            }

            SelectSnapPos(StartMovementEventType.Touch);
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

            int axis = (int)m_ScrollWheelDirection;
            int inverseAxis = 1 - axis;

            Vector2 delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
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
            
            Vector2 snapPos = FindClosestSnapPositionToPosition(referencePos, m_Velocity);
            snapPos.x = Mathf.Clamp(snapPos.x, m_MinPos.x, m_MaxPos.x);
            snapPos.y = Mathf.Clamp(snapPos.y, m_MinPos.y, m_MaxPos.y);

            float decelRate = m_Scroller.CalculateDecelerationRate(m_Velocity.magnitude, Vector2.Distance(snapPos, m_Content.anchoredPosition));
            float duration = m_Scroller.CalculateDuration(Mathf.Abs(m_Velocity.magnitude), decelRate);
            duration = Mathf.Clamp(duration, m_MinDuration, m_MaxDuration);

            m_Scroller.StartScroll(m_Content.anchoredPosition, snapPos, duration,interpolator);

            m_TargetItemSelected.Invoke(GetClosestSnappableChildToPosition(snapPos));
        }
        #endregion

        #region Public Functions

        /// <summary>
        /// Gets the snappable closest child RectTransform to the normalized position of the content.
        /// </summary>
        /// <param name="normalizedPosition">Normalized position of the content.</param>
        /// <returns>Closest snappable child of the content.</returns>
        public RectTransform GetClosestSnappableChildToNormalizedPosition(Vector2 normalizedPosition)
        {
            Vector2 anchorPos = m_Content.anchoredPosition;
            SetNormalizedPosition(normalizedPosition);
            Vector2 closestSnapToPosition = FindClosestSnapPositionToPosition(m_Content.anchoredPosition);
            int index = m_SnapPositions.IndexOf(closestSnapToPosition);
            return m_AvailableForSnappingTo[index];
        }

        /// <summary>
        /// Gets the closest child RectTransform to the position of the content.
        /// </summary>
        /// <param name="position">Position of the content in the content's local space.</param>
        /// <returns>Closest child of the content.</returns>
        public RectTransform GetClosestSnappableChildToPosition(Vector2 position)
        {
            if (m_AvailableForSnappingTo.Count == 0)
            {
                return null;
            }
            Vector2 closestSnapToPosition = FindClosestSnapPositionToPosition(position);
            int index = m_SnapPositions.IndexOf(closestSnapToPosition);
            return m_AvailableForSnappingTo[index];
        }

        /// <summary>
        /// Gets the normalized position of the content when it is snapped to the child.
        /// </summary>
        /// <returns>Returns true if the supplied RectTransform is a child of the content.</returns>
        public bool GetNormalizedPositionOfChild(RectTransform child, out Vector2 normalizedPos)
        {
            float distanceX = DistanceOnAxis(child.anchoredPosition, leftChild.anchoredPosition, 0);
            float distanceY = DistanceOnAxis(child.anchoredPosition, topChild.anchoredPosition, 1);
            Vector2 scrollableSize = m_ContentBounds.size - m_ViewBounds.size;
            normalizedPos = new Vector2(distanceX / scrollableSize.x, distanceY / scrollableSize.y);
            return child.parent == m_Content;
        }

        /// <summary>
        /// Gets the position of the content, in the content's local space, when it is snapped to the child.
        /// </summary>
        /// <returns>Returns true if the supplied RectTransform is a child of the content.</returns>
        public bool GetPositionOfChild(RectTransform child, out Vector2 position)
        {
            Vector2 anchoredPos = m_Content.anchoredPosition;
            Vector2 normalizedPos;
            GetNormalizedPositionOfChild(child, out normalizedPos);
            SetNormalizedPosition(normalizedPos);
            position = m_Content.anchoredPosition;
            m_Content.anchoredPosition = anchoredPos;
            return child.parent == m_Content;
        }

        /// <summary>
        /// Insert a new child into the Scroll Snap.
        /// </summary>
        /// <param name="child">The child you would like to insert.</param>
        /// <param name="worldPos">The world position you want to insert the child at.</param>
        /// <param name="snappable">If the new child should be snappable.</param>
        public void InsertChild(RectTransform child, Vector3 worldPos, bool snappable)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(m_ContentWorldCorners[1], m_Content.rotation, m_Content.localScale);
            Vector2 posRelativeToContentTopLeft = matrix.inverse.MultiplyPoint3x4(worldPos);

            SetParentToContent(child);
            child.anchoredPosition = posRelativeToContentTopLeft;
            
            if (m_FilterModeForCalculatingSize == FilterMode.WhiteList)
            {
                m_CalculatingFilter.Add(child);
            }
            m_AvailableForCalculating.Add(child);

            if (snappable)
            {
                if (m_FilterModeForSnapPositions == FilterMode.WhiteList)
                {
                    m_SnapPositionsFilter.Add(child);
                }
                m_AvailableForSnappingTo.Add(child);
            }
            else
            {
                if (m_FilterModeForSnapPositions == FilterMode.BlackList)
                {
                    m_SnapPositionsFilter.Add(child);
                }
            }

            m_UpdateChildren = true;
            m_UpdateContentSize = true;
            m_UpdateSnapPositions = true;
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
                m_Content.anchoredPosition = contentPosition;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Changes the relative (relative to unmoved children) snap position and position of the provided children by the offset.
        /// </summary>
        /// <param name="offset">The amount to move the children by.</param>
        /// <param name="children">The children to move.</param>
        public void MoveChildren(Vector2 offset, params RectTransform[] children)
        {
            foreach (RectTransform child in children)
            {
                if (child != null)
                {
                    child.anchoredPosition += offset;
                }
            }

            m_UpdateChildren = true;
            m_UpdateContentSize = true;
            m_UpdateSnapPositions = true;
        }

        /// <summary>
        /// Changes the relative (relative to unmoved children) snap position and position of the children that are to the direction (left of, right of, above, or below) of the referenceChild by the offset.
        /// </summary>
        /// <param name="referenceChild">The child the direction is relative to.</param>
        /// <param name="direction">The relative direction of the children being moved. i.e. left of referenceChild, right of refrenceChild, above referenceChild, or below referenceChild</param>
        /// <param name="offset">The amount to move the children by.</param>
        public void MoveChildren(RectTransform referenceChild, RelativeDirection direction, Vector2 offset)
        {
            foreach (RectTransform child in m_AvailableForCalculating)
            {
                switch (direction)
                {
                    case RelativeDirection.Above:
                        if (referenceChild.anchoredPosition.y < child.anchoredPosition.y)
                        {
                            child.anchoredPosition += offset;
                        }
                        break;
                    case RelativeDirection.Below:
                        if (referenceChild.anchoredPosition.y > child.anchoredPosition.y)
                        {
                            child.anchoredPosition += offset;
                        }
                        break;
                    case RelativeDirection.Left:
                        if (referenceChild.anchoredPosition.x > child.anchoredPosition.x)
                        {
                            child.anchoredPosition += offset;
                        }
                        break;
                    case RelativeDirection.Right:
                        if (referenceChild.anchoredPosition.x < child.anchoredPosition.x)
                        {
                            child.anchoredPosition += offset;
                        }
                        break;
                }

            }

            m_UpdateChildren = true;
            m_UpdateContentSize = true;
            m_UpdateSnapPositions = true;
        }

        /// <summary>
        /// Remove a calculable child from the Scroll Snap, the child will be deleted.
        /// </summary>
        /// <param name="child">The child to remove.</param>
        public void RemoveChild(RectTransform child)
        {
            Remove(child);
            Destroy(child.gameObject);
        }

        /// <summary>
        /// Remove a calculable child from the Scroll Snap, the child will be reparented to the newParent.
        /// </summary>
        /// <param name="child">The child to remove.</param>
        /// <param name="newParent">The RectTransform the child will be parented to, if the newParent is null the new child will be reparented to the parent Canvas.</param>
        public void RemoveChild(RectTransform child, RectTransform newParent)
        {
            Remove(child);

            if (newParent == null)
            {
                child.SetParent(GetCanvasTransform(child.parent));
            }
            else
            {
                child.SetParent(newParent);
            }
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
            if(child == null)
            {
                return false;
            }

            Vector2 contentPosition;
            GetPositionOfChild(child, out contentPosition);
            if (m_AvailableForSnappingTo.Contains(child))
            {
                m_Scroller.StartScroll(m_Content.anchoredPosition, contentPosition, duration, interpolator);

                m_StartMovementEvent.Invoke(StartMovementEventType.Programmatic);
                m_TargetItemSelected.Invoke(child);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Scrolls to the nearest snap position to the normalized position in the specified duration of time.
        /// </summary>
        /// <param name="normalizedPos">The reference end position of the content, normalized.</param>
        /// <param name="duration">The duration of the scroll in seconds.</param>
        /// <param name="interpolator">Modifies the animation.</param>
        public void ScrollToNearestSnapPosToNormalizedPos(Vector2 normalizedPos, float duration, Interpolator interpolator)
        {
            m_StartMovementEvent.Invoke(StartMovementEventType.Programmatic);
            m_Scroller.ForceFinish();
            Vector2 anchoredPos = m_Content.anchoredPosition;
            SetNormalizedPosition(normalizedPos);
            Vector2 targetPosition = FindClosestSnapPositionToPosition(m_Content.anchoredPosition);
            m_Scroller.StartScroll(anchoredPos, targetPosition, duration, interpolator);
            m_TargetItemSelected.Invoke(GetClosestSnappableChildToPosition(targetPosition));
        }

        /// <summary>
        /// Scrolls to the nearest snap position to the end position in the specified duration of time.
        /// </summary>
        /// <param name="endPos">The reference end position of the content, in the content's local coordinates.</param>
        /// <param name="duration">The duration of the scroll in seconds.</param>
        /// <param name="interpolator">Modifies the animation.</param>
        public void ScrollToNearestSnapPosToPos(Vector2 endPos, float duration, Interpolator interpolator)
        {
            m_StartMovementEvent.Invoke(StartMovementEventType.Programmatic);
            m_Scroller.ForceFinish();
            Vector2 targetPosition = FindClosestSnapPositionToPosition(endPos);
            m_Scroller.StartScroll(m_Content.anchoredPosition, targetPosition, duration, interpolator);
            m_TargetItemSelected.Invoke(GetClosestSnappableChildToPosition(targetPosition));
        }

        /// <summary>
        /// Set the position of the child relative to the current top left of the content.
        /// </summary>
        /// <param name="child">The child to reposition.</param>
        /// <param name="posRelativeToTopLeft">The child's new position relative to the top left of the content.</param>
        public void SetChildPos(RectTransform child, Vector2 posRelativeToTopLeft)
        {
            child.anchoredPosition = posRelativeToTopLeft;

            m_UpdateChildren = true;
            m_UpdateContentSize = true;
            m_UpdateSnapPositions = true;
        }

        /// <summary>
        /// Set the position of the child in world coordinates.
        /// </summary>
        /// <param name="child">The child to reposition.</param>
        /// <param name="worldPos">The child's new position in world coordinates.</param>
        public void SetChildPos(RectTransform child, Vector3 worldPos)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(m_ContentWorldCorners[1], m_Content.rotation, m_Content.localScale);
            Vector2 posRelativeToContentTopLeft = matrix.inverse.MultiplyPoint3x4(worldPos);

            child.anchoredPosition = posRelativeToContentTopLeft;

            m_UpdateChildren = true;
            m_UpdateContentSize = true;
            m_UpdateSnapPositions = true;

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
            if (m_AvailableForSnappingTo.Contains(child) == snappable || !m_AvailableForCalculating.Contains(child))
            {
                return;
            }

            if (snappable)
            {
                if (m_FilterModeForSnapPositions == FilterMode.WhiteList)
                {
                    m_SnapPositionsFilter.Add(child);
                }
                else
                {
                    m_SnapPositionsFilter.Remove(child);
                }

                m_AvailableForSnappingTo.Add(child);
                
                m_UpdateChildren = true;
                m_UpdateContentSize = true;
                m_UpdateSnapPositions = true;
            }
            else
            {
                if (m_FilterModeForSnapPositions == FilterMode.BlackList)
                {
                    m_SnapPositionsFilter.Add(child);
                }
                else
                {
                    m_SnapPositionsFilter.Remove(child);
                }

                m_ChildrenForSnappingFromLeftToRight.Remove(child);
                m_ChildrenForSnappingFromTopToBottom.Remove(child);
                m_AvailableForSnappingTo.Remove(child);

                m_UpdateContentSize = true;
                m_UpdateSnapPositions = true;
            }
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
        #endregion

        #region Calculations
        private Vector2 FindClosestSnapPositionToPosition(Vector2 position, Vector2 direction)
        {
            EnsureLayoutHasRebuilt();

            float averageVelocityPerDegree = m_Velocity.magnitude / 180;
            Vector2 selected = Vector2.zero;
            float lowestValue = Mathf.Infinity;

            foreach (Vector2 snapPosition in m_SnapPositions)
            {
                float distance = Vector2.Distance(snapPosition, position);
                float angle = Vector2.Angle(direction, snapPosition - m_ContentStartPosition);
                float value = (distance + (angle * averageVelocityPerDegree)) / 2;

                if (value < lowestValue)
                {
                    lowestValue = value;
                    selected = snapPosition;
                }
            }

            return selected;
        }

        private Vector2 FindClosestSnapPositionToPosition(Vector2 position)
        {
            EnsureLayoutHasRebuilt();

            Vector2 selected = Vector2.zero;
            float shortestDistance = Mathf.Infinity;

            foreach (Vector2 snapPosition in m_SnapPositions)
            {
                float distance = Vector2.Distance(snapPosition, position);

                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    selected = snapPosition;
                }
            }

            return selected;
        }

        private float DistanceOnAxis(Vector2 posOne, Vector2 posTwo, int axis)
        {
            return Mathf.Abs(posOne[axis] - posTwo[axis]);
        }

        private float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        private Vector2 CalculateOffset(Vector2 delta)
        {
            Vector2 offset = Vector2.zero;

            Vector2 min = m_ContentBounds.min;
            Vector2 max = m_ContentBounds.max;

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

            return offset;
        }
        
        private float Hypot(float x, float y)
        {
            return Mathf.Sqrt(x * x + y * y);
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
            if (contentIsLayoutGroup)
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

        private void SetParentToContent(RectTransform child)
        {
            child.SetParent(m_Content, false);
            m_Tracker.Add(this, child, DrivenTransformProperties.Anchors);
            child.anchorMax = new Vector2(0, 1);
            child.anchorMin = new Vector2(0, 1);
        }

        private void SetHorizontalNormalizedPosition(float value) { SetNormalizedPosition(value, 0); }
        private void SetVerticalNormalizedPosition(float value) { SetNormalizedPosition(value, 1); }
        private void SetNormalizedPosition(Vector2 value) { SetNormalizedPosition(value.x, 0); SetNormalizedPosition(value.y, 1); }

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
            bool setRef = false;
            if (closestItem != null && (m_UpdateChildren || m_UpdateContentSize || m_UpdateSnapPositions))
            {
                SetReferencePos(closestItem);
                setRef = true;
            }

            if (m_UpdateChildren)
            {
                GetChildrenFromStartToEnd();
                m_UpdateChildren = false;
            }

            if (m_UpdateContentSize)
            {
                ResizeContent();
                m_UpdateContentSize = false;
            }

            if (m_UpdateSnapPositions)
            {
                GetSnapPositions();
                m_UpdateSnapPositions = false;
            }

            if (setRef)
            {
                ResetContentPos();
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
            m_PrevClosestItem = m_ClosestItem;
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

        private void Remove(RectTransform child)
        {
            if (child == null || !m_AvailableForCalculating.Contains(child))
            {
                return;
            }
            
            if (m_ChildrenForSizeFromLeftToRight.Count == 0 || m_ChildrenForSizeFromTopToBottom.Count == 0 || child == leftChild || child == topChild || child == rightChild || child == bottomChild)
            {
                m_UpdateContentSize = true;
            }
            m_UpdateSnapPositions = true;

            m_ChildrenForSizeFromLeftToRight.Remove(child);
            m_ChildrenForSizeFromTopToBottom.Remove(child);
            m_AvailableForCalculating.Remove(child);

            m_CalculatingFilter.Remove(child);
            m_SnapPositionsFilter.Remove(child);

            if (m_AvailableForSnappingTo.Contains(child))
            {
                m_ChildrenForSnappingFromLeftToRight.Remove(child);
                m_ChildrenForSnappingFromTopToBottom.Remove(child);
                m_AvailableForSnappingTo.Remove(child);
            }
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
        /// This method is called before the OmniDirectional Scroll Snap starts any animations.
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

                Vector3[] childCorners = new Vector3[4];
                foreach (RectTransform child in m_Content)
                {
                    child.GetWorldCorners(childCorners);
                    if (m_AvailableForSnappingTo.Contains(child))
                    {
                        Gizmos.color = Color.cyan;

                        Gizmos.DrawRay(child.position, leftDirection.normalized * GetGizmoSize(child.position) * .25f);
                        Gizmos.DrawRay(child.position, -(leftDirection.normalized * GetGizmoSize(child.position) * .25f));
                        Gizmos.DrawRay(child.position, topDirection.normalized * GetGizmoSize(child.position) * .25f);
                        Gizmos.DrawRay(child.position, -(topDirection.normalized * GetGizmoSize(child.position) * .25f));
                    }
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
            m_Friction = Mathf.Clamp(m_Friction, .001f, .999f);
            m_Tension = Mathf.Max(m_Tension, 0);
            m_MinDuration = Mathf.Max(m_MinDuration, .001f);
            m_MaxDuration = Math.Max(m_MaxDuration, Mathf.Max(m_MinDuration, .001f));

            m_ScrollBarFriction = Mathf.Clamp(m_ScrollBarFriction, .1f, .999f);
            m_ScrollBarTension = Mathf.Max(m_ScrollBarTension, 0);

            m_ScrollSensitivity = Mathf.Max(m_ScrollSensitivity, 0);
            m_ScrollDelay = Mathf.Max(scrollDelay, 0);
            m_ScrollFriction = Mathf.Clamp(m_ScrollFriction, 1f, .999f);
            m_ScrollTension = Mathf.Max(m_ScrollTension, 0);

            SetDirtyCaching();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Validate();
        }

        [MenuItem("GameObject/UI/ScrollSnaps/OmniDirectionalScrollSnap", false, 10)]
        private static void CreateOmniDirectionalScrollSnap(MenuCommand menuCommand)
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

            GameObject GO = new GameObject("OmniDirectional Scroll Snap");
            RectTransform rectTransform = GO.AddComponent<RectTransform>();
            OmniDirectionalScrollSnap scrollSnap = GO.AddComponent<OmniDirectionalScrollSnap>();
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
            contentRectTransform.sizeDelta = new Vector2(200 + (150 * (numChildren - 1)), 200 + (150 * (numChildren - 1)));
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
                childRectTransform.anchoredPosition = new Vector2(100 + (150 * i), -(100 + (150 * i)));
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