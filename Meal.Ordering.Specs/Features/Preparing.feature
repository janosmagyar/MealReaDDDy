Feature: Preparing items
- For an Item to be prepared needs to be confirmed by every piece when expeditor putting it into the bag or on a tray
- A Meal stays 'InPreparation' until all items are not prepared

Background:
    Given an order
        | Order number | State         | Serving | Table |
        | 23           | InPreparation | tray    | 99    |

Scenario: Preparing one item
    Given the items in the order are
        | Count | Name      | Category | Prepared |
        | 2     | hamburger | burger   | 0        |

    When expeditor confirms that one piece from '1st' item is prepared

    Then the items in the order are
        | Count | Name      | Category | Prepared |
        | 2     | hamburger | burger   | false    |

Scenario: Preparing one item fully

    Given the items in the order are
        | Count | Name      | Category | Prepared |
        | 2     | hamburger | burger   | 0        |
        | 1     | mid coke  | drink    | 0        |

    When expeditor confirms that one piece from '1st' item is prepared
    And expeditor confirms that one piece from '1st' item is prepared

    Then the items in the order are
        | Count | Name      | Category | Prepared |
        | 2     | hamburger | burger   | true     |
        | 1     | mid coke  | drink    | false    |

Scenario: Preparing all items fully
    Given the items in the order are
        | Count | Name      | Category | Prepared |
        | 2     | hamburger | burger   | 0        |
        | 1     | mid coke  | drink    | 0        |

    When expeditor confirms that one piece from '1st' item is prepared
    And expeditor confirms that one piece from '1st' item is prepared
    And expeditor confirms that one piece from '2nd' item is prepared

    Then the order
        | Order number | State | Serving | Table |
        | 23           | Ready | tray    | 99    |

    Then the items in the order are
        | Count | Name      | Category | Prepared |
        | 2     | hamburger | burger   | true     |
        | 1     | mid coke  | drink    | true     |

Scenario: Preparing an item more then ordered
    Given the items in the order are
        | Count | Name      | Category | Prepared |
        | 2     | hamburger | burger   | 0        |
        | 1     | mid coke  | drink    | 0        |

    When expeditor confirms that one piece from '2nd' item is prepared
    And expeditor confirms that one piece from '2nd' item is prepared

    Then the error message is 'This item is already prepared!'

Scenario Outline: Preparing an invalid item
    Given the items in the order are
        | Count | Name      | Category | Prepared |
        | 2     | hamburger | burger   | 0        |
        | 1     | mid coke  | drink    | 0        |

    When expeditor confirms that one piece from '<item_index>' item is prepared

    Then the error message is 'Invalid item index!'

Examples:
    | item_index |
    | 3rd        |
    | 136th      |

Scenario: Wrong command
    Given an order
        | Order number | State | Serving | Table |
        | 23           | Ready | tray    | 99    |
    Given the items in the order are
        | Count | Name      | Category | Prepared |
        | 2     | hamburger | burger   | 2        |
        | 1     | mid coke  | drink    | 1        |

    When expeditor confirms that one piece from '1st' item is prepared

    Then the error message is 'Invalid command for state!'
