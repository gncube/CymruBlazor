# Review Code

Perform a production-ready code review.

## Workflow

```
Architect (Design & Architecture)
    ↓
Code Reviewer (Implementation Quality)
    ↓
Security Specialist (Security & Access Control)
    ↓
Testing Specialist (Testability & Coverage)
    ↓
Report Generation
    ↓
Deliver
```

## Review Criteria

### Design & Architecture

- Follows vertical slice architecture principles
- SOLID principles applied correctly
- Proper separation of concerns (API → Application → Domain → Infrastructure)
- CQS boundaries respected
- Mediator pattern used correctly if applicable
- Domain model correctness and invariant enforcement
- Clear dependency directions

### Implementation Quality

- Modern C# idioms (primary constructors, collection expressions, file-scoped namespaces)
- Naming clarity and consistency
- Code complexity and readability
- Duplication or boilerplate opportunities
- Proper use of Result<T> for error handling
- Resource disposal and lifetime management
- No static analyzer warnings (CA1510, CA2227, CA1805, etc.)

### Security & Access Control

- Authentication and authorization enforcement
- Input validation and sanitization
- SQL injection prevention (parameterized queries)
- XSS prevention in Blazor components
- Credential and secret handling
- Proper use of HttpOnly cookies
- CORS configuration appropriate

### Testability & Coverage

- Unit test coverage for business logic
- Integration tests for endpoints
- Handler/query tests in place
- Edge cases and error paths covered
- Mocking strategies appropriate
- Test names clearly describe intent
- Arrange-Act-Assert pattern followed

### Documentation

- Public API documented (XML comments)
- Complex logic explained
- ADR created if architectural decisions made
- README/feature docs updated if user-facing
- API documentation current

## Workflow Phases

### Phase 1: Architect (Design & Architecture)

- Analyze design decisions and patterns
- Verify SOLID principles
- Check separation of concerns
- Validate domain model integrity
- Document architectural findings

### Phase 2: Code Reviewer (Implementation Quality)

- Review code style and modern C# usage
- Check for duplication and complexity
- Verify naming conventions
- Analyze error handling patterns
- Identify performance opportunities
- Highlight maintenance concerns

### Phase 3: Security Specialist (Security & Access Control)

- Audit authorization enforcement
- Verify input validation
- Check credential handling
- Validate CORS and HTTPS usage
- Identify potential security gaps

### Phase 4: Testing Specialist (Testability & Coverage)

- Analyze unit test coverage
- Review test quality and clarity
- Check integration test completeness
- Verify edge case handling
- Recommend additional tests if needed

### Phase 5: Report Generation

**Deliverables:**

Structured code review report with:

1. **Summary**
   - Overall assessment (Approve, Needs Changes, Reject)
   - Key strengths
   - Major concerns

2. **Findings (by category)**
   - Finding ID and category
   - Severity (Critical, High, Medium, Low, Informational)
   - Description of issue
   - Location (file, line, method)
   - Recommendation for fix
   - Trade-offs if applicable
   - Suggested implementation

3. **Detailed Analysis**
   - Design review results
   - Implementation quality assessment
   - Security audit findings
   - Test coverage analysis

4. **Statistics**
   - Files changed
   - Complexity metrics
   - Test coverage percentage

### Phase 6: Deliver

- Review report complete and actionable
- All findings documented with severity and recommendations
- Clear path forward for addressing issues
- Ready for developer action and conversation
